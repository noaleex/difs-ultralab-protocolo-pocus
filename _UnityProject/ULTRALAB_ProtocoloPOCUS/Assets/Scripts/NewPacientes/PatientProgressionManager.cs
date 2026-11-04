using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla a progressão e a invocação dos três pacientes.
/// </summary>
public class PatientProgressionManager : MonoBehaviour
{
    [Header("Paciente 1 já presente na cena")]
    [SerializeField] private OpenExams patient1InScene;

    [Header("Prefabs dos pacientes seguintes")]
    [SerializeField] private GameObject patient2Prefab;
    [SerializeField] private GameObject patient3Prefab;
    [SerializeField] private Transform spawnParent;

    [Header("Limites de Welfare")]
    [SerializeField, Min(0)] private int spawnPatient2At = 50;
    [SerializeField, Min(1)] private int dischargeAt = 74;

    private PatientData patient1Data;
    private PatientData patient2Data;
    private PatientData patient3Data;

    private OpenExams patient2Instance;
    private OpenExams patient3Instance;

    private bool reconcileAfterDestroyQueued;

    // =====================================================
    // EVENTOS
    // =====================================================

    private void OnEnable()
    {
        GameSession.DayChanged += OnDayChanged;
    }

    private void OnDisable()
    {
        GameSession.DayChanged -= OnDayChanged;
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        if (patient1InScene == null)
        {
            Debug.LogError(
                "[PatientProgressionManager] " +
                "Atribua Patient 1 In Scene no Inspector.",
                this
            );

            enabled = false;
            return;
        }

        patient1Data = patient1InScene.PatientDataReference;

        patient2Data = GetPatientDataFromPrefab(
            patient2Prefab,
            "Paciente 2"
        );

        patient3Data = GetPatientDataFromPrefab(
            patient3Prefab,
            "Paciente 3"
        );

        if (patient1Data == null ||
            patient2Data == null ||
            patient3Data == null)
        {
            Debug.LogError(
                "[PatientProgressionManager] " +
                "Configure PatientData nos três pacientes.",
                this
            );

            enabled = false;
            return;
        }

        GameSession.ConfigurePatientProgression(
            patient1Data,
            patient2Data,
            patient3Data,
            spawnPatient2At
        );

        // A progressão dos pacientes ocorre somente quando o dia muda.
        // Não reconciliar aqui para evitar alta/destruição durante a inicialização.
    }

    // =====================================================
    // OBTER PATIENT DATA
    // =====================================================

    private static PatientData GetPatientDataFromPrefab(
        GameObject prefab,
        string label)
    {
        if (prefab == null)
        {
            Debug.LogError(
                $"[PatientProgressionManager] Prefab de {label} não atribuído."
            );

            return null;
        }

        OpenExams openExams =
            prefab.GetComponent<OpenExams>();

        if (openExams == null)
        {
            Debug.LogError(
                $"[PatientProgressionManager] " +
                $"O prefab de {label} precisa ter OpenExams na raiz.",
                prefab
            );

            return null;
        }

        return openExams.PatientDataReference;
    }

    // =====================================================
    // EVENTOS DE DIA E WELFARE
    // =====================================================

    private void OnDayChanged(int day)
    {
        ReconcileSceneObjects();
    }

    // =====================================================
    // RECONCILIAR PACIENTES
    // =====================================================

    private void ReconcileSceneObjects()
    {
        if (GameEndingManager.IsEnding)
            return;

        if (patient1Data == null ||
            patient2Data == null ||
            patient3Data == null)
            return;

        if (GameSession.GetPatientWelfare(patient1Data) <= 0 ||
            (GameSession.IsPatient2Unlocked &&
             GameSession.GetPatientWelfare(patient2Data) <= 0) ||
            (GameSession.IsPatient3Unlocked &&
             GameSession.GetPatientWelfare(patient3Data) <= 0))
        {
            GameEndingManager.FinishGame(
                GameEndingManager.EndingType.Negative
            );

            return;
        }

        bool requestedDestroy = false;

        // =================================================
        // PACIENTE 1
        // =================================================

        if (GameSession.GetPatientWelfare(patient1Data) >= dischargeAt &&
            !GameSession.IsPatientDischarged(patient1Data))
        {
            GameSession.MarkPatientAsDischarged(patient1Data);
        }

        if (GameSession.IsPatientDischarged(patient1Data))
        {
            if (patient1InScene != null)
            {
                Destroy(patient1InScene.gameObject);
                requestedDestroy = true;

                Debug.Log(
                    "[PatientProgression] Paciente 1 recebeu alta e será destruído."
                );
            }
        }

        // =================================================
        // PACIENTE 2
        // =================================================

        if (GameSession.IsPatient2Unlocked &&
            !GameSession.IsPatientDischarged(patient2Data))
        {
            if (GameSession.GetPatientWelfare(patient2Data) >= dischargeAt)
            {
                GameSession.MarkPatientAsDischarged(patient2Data);
            }
            else
            {
                patient2Instance = EnsurePatientExists(
                    patient2Data,
                    patient2Prefab,
                    patient2Instance,
                    "Paciente 2"
                );
            }
        }

        if (GameSession.IsPatientDischarged(patient2Data))
        {
            if (patient2Instance != null)
            {
                Destroy(patient2Instance.gameObject);

                patient2Instance = null;
                requestedDestroy = true;

                Debug.Log(
                    "[PatientProgression] Paciente 2 recebeu alta e será destruído."
                );
            }
        }

        // =================================================
        // PACIENTE 3
        // =================================================

        bool firstTwoDischarged =
            GameSession.IsPatientDischarged(patient1Data) &&
            GameSession.IsPatientDischarged(patient2Data);

        bool firstTwoDestroyed =
            patient1InScene == null &&
            patient2Instance == null;

        if (GameSession.IsPatient3Unlocked &&
            firstTwoDischarged &&
            firstTwoDestroyed &&
            !GameSession.IsPatientDischarged(patient3Data))
        {
            if (GameSession.GetPatientWelfare(patient3Data) >= dischargeAt)
            {
                GameSession.MarkPatientAsDischarged(patient3Data);
            }
            else
            {
                patient3Instance = EnsurePatientExists(
                    patient3Data,
                    patient3Prefab,
                    patient3Instance,
                    "Paciente 3"
                );
            }
        }

        // =================================================
        // DESTRUIR PACIENTE 3 APÓS ALTA
        // =================================================

        if (GameSession.IsPatientDischarged(patient3Data) &&
            patient3Instance != null)
        {
            Destroy(patient3Instance.gameObject);
            patient3Instance = null;

            requestedDestroy = true;
        }

        // =================================================
        // FIM POSITIVO
        // =================================================

        if (GameSession.IsPatientDischarged(patient1Data) &&
            GameSession.IsPatientDischarged(patient2Data) &&
            GameSession.IsPatientDischarged(patient3Data))
        {
            GameEndingManager.FinishGame(
                GameEndingManager.EndingType.Positive
            );

            return;
        }

        // =================================================
        // REVERIFICAR APÓS DESTROY
        // =================================================

        if (requestedDestroy)
        {
            ScheduleReconcileAfterDestroy();
        }
    }

    // =====================================================
    // RECONCILIAR APÓS DESTRUIÇÃO
    // =====================================================

    private void ScheduleReconcileAfterDestroy()
    {
        if (reconcileAfterDestroyQueued)
            return;

        reconcileAfterDestroyQueued = true;

        StartCoroutine(ReconcileAfterDestroy());
    }

    private IEnumerator ReconcileAfterDestroy()
    {
        // Espera o Unity efetivar Destroy().
        yield return null;

        reconcileAfterDestroyQueued = false;

        if (this != null && isActiveAndEnabled)
        {
            ReconcileSceneObjects();
        }
    }

    // =====================================================
    // GARANTIR EXISTÊNCIA DO PACIENTE
    // =====================================================

    private OpenExams EnsurePatientExists(
        PatientData data,
        GameObject prefab,
        OpenExams cached,
        string label)
    {
        if (cached != null)
            return cached;

        if (PatientManager.Instance != null)
        {
            IReadOnlyList<OpenExams> active =
                PatientManager.Instance.GetPatients();

            foreach (OpenExams candidate in active)
            {
                if (candidate != null &&
                    candidate.PatientDataReference == data)
                {
                    return candidate;
                }
            }
        }

        if (prefab == null)
            return null;

        GameObject instance = spawnParent != null
            ? Instantiate(prefab, spawnParent, false)
            : Instantiate(prefab);

        OpenExams spawned = instance.GetComponent<OpenExams>();

        if (spawned == null)
        {
            Debug.LogError(
                $"[PatientProgressionManager] " +
                $"O prefab de {label} não possui OpenExams na raiz.",
                instance
            );

            Destroy(instance);
            return null;
        }

        Debug.Log(
            $"[PatientProgression] {label} foi invocado."
        );

        return spawned;
    }
}
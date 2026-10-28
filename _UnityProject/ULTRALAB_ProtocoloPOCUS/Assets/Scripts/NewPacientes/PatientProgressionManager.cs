using System.Collections.Generic;
using UnityEngine;

/// <summary>Reconstrói a apresentação dos pacientes ao voltar da cena de exames.</summary>
public class PatientProgressionManager : MonoBehaviour
{
    [Header("Paciente 1 já presente na cena") ]
    [SerializeField] private OpenExams patient1InScene;

    [Header("Prefabs dos pacientes seguintes (OpenExams no objeto raiz)")]
    [SerializeField] private GameObject patient2Prefab;
    [SerializeField] private GameObject patient3Prefab;
    [SerializeField] private Transform spawnParent;

    [Header("Limites de welfare") ]
    [SerializeField, Min(0)] private int spawnPatient2At = 50;
    [SerializeField, Min(1)] private int dischargeAt = 74;

    private PatientData patient1Data;
    private PatientData patient2Data;
    private PatientData patient3Data;
    private OpenExams patient2Instance;
    private OpenExams patient3Instance;

    private void OnEnable()
    {
        GameSession.PatientWelfareChanged += OnPatientWelfareChanged;
        GameSession.DayChanged += OnDayChanged;
    }

    private void OnDisable()
    {
        GameSession.PatientWelfareChanged -= OnPatientWelfareChanged;
        GameSession.DayChanged -= OnDayChanged;
    }

    private void Start()
    {
        if (patient1InScene == null)
        {
            Debug.LogError("[PatientProgressionManager] Atribua Patient 1 In Scene no Inspector.", this);
            enabled = false;
            return;
        }

        patient1Data = patient1InScene.PatientDataReference;
        patient2Data = GetPatientDataFromPrefab(patient2Prefab, "Paciente 2");
        patient3Data = GetPatientDataFromPrefab(patient3Prefab, "Paciente 3");

        if (patient1Data == null || patient2Data == null || patient3Data == null)
        {
            Debug.LogError("[PatientProgressionManager] Configure PatientData nos três pacientes/prefabs.", this);
            enabled = false;
            return;
        }

        GameSession.ConfigurePatientProgression(patient1Data, patient2Data, patient3Data, spawnPatient2At);
        ReconcileSceneObjects();
    }

    private static PatientData GetPatientDataFromPrefab(GameObject prefab, string label)
    {
        if (prefab == null)
        {
            Debug.LogError($"[PatientProgressionManager] Prefab de {label} não foi atribuído.");
            return null;
        }

        OpenExams openExams = prefab.GetComponent<OpenExams>();
        if (openExams == null)
        {
            Debug.LogError($"[PatientProgressionManager] O prefab de {label} precisa ter OpenExams no objeto raiz.", prefab);
            return null;
        }
        return openExams.PatientDataReference;
    }

    private void OnDayChanged(int day)
    {
        // Reconcile only when the clock advances to a new day.
        ReconcileSceneObjects();
    }

    private void OnPatientWelfareChanged(PatientData patient, int previous, int current)
    {
        if (GameEndingManager.IsEnding) return;
        if (patient == patient1Data || patient == patient2Data || patient == patient3Data)
            ReconcileSceneObjects();
    }

    private void ReconcileSceneObjects()
    {
        if (patient1Data == null || patient2Data == null || patient3Data == null) return;

        if (GameSession.GetPatientWelfare(patient1Data) <= 0 ||
            (GameSession.IsPatient2Unlocked && GameSession.GetPatientWelfare(patient2Data) <= 0) ||
            (GameSession.IsPatient3Unlocked && GameSession.GetPatientWelfare(patient3Data) <= 0))
        {
            GameEndingManager.FinishGame(GameEndingManager.EndingType.Negative);
            return;
        }

        if (GameSession.GetPatientWelfare(patient1Data) >= dischargeAt && !GameSession.IsPatientDischarged(patient1Data))
            GameSession.MarkPatientAsDischarged(patient1Data);

        if (GameSession.IsPatientDischarged(patient1Data))
        {
            if (patient1InScene != null) Destroy(patient1InScene.gameObject);
        }

        if (GameSession.IsPatient2Unlocked && !GameSession.IsPatientDischarged(patient2Data))
        {
            if (GameSession.GetPatientWelfare(patient2Data) >= dischargeAt)
                GameSession.MarkPatientAsDischarged(patient2Data);
            else
                patient2Instance = EnsurePatientExists(patient2Data, patient2Prefab, patient2Instance, "Paciente 2");
        }
        if (GameSession.IsPatientDischarged(patient2Data) && patient2Instance != null)
        {
            Destroy(patient2Instance.gameObject);
            patient2Instance = null;
        }

        if (GameSession.IsPatient3Unlocked && !GameSession.IsPatientDischarged(patient3Data))
        {
            if (GameSession.GetPatientWelfare(patient3Data) >= dischargeAt)
                GameSession.MarkPatientAsDischarged(patient3Data);
            else
                patient3Instance = EnsurePatientExists(patient3Data, patient3Prefab, patient3Instance, "Paciente 3");
        }
        if (GameSession.IsPatientDischarged(patient3Data) && patient3Instance != null)
        {
            Destroy(patient3Instance.gameObject);
            patient3Instance = null;
        }

        if (GameSession.IsPatientDischarged(patient3Data) &&
            GameSession.IsPatientDischarged(patient2Data) &&
            GameSession.IsPatientDischarged(patient1Data))
        {
            GameEndingManager.FinishGame(GameEndingManager.EndingType.Positive);
        }
    }

    private OpenExams EnsurePatientExists(PatientData data, GameObject prefab, OpenExams cached, string label)
    {
        if (cached != null) return cached;

        if (PatientManager.Instance != null)
        {
            IReadOnlyList<OpenExams> active = PatientManager.Instance.GetPatients();
            foreach (OpenExams candidate in active)
            {
                if (candidate != null && candidate.PatientDataReference == data)
                    return candidate;
            }
        }

        if (prefab == null) return null;
        GameObject instance = spawnParent != null ? Instantiate(prefab, spawnParent, false) : Instantiate(prefab);
        OpenExams spawned = instance.GetComponent<OpenExams>();
        if (spawned == null)
        {
            Debug.LogError($"[PatientProgressionManager] O prefab de {label} não tem OpenExams na raiz.", instance);
            Destroy(instance);
            return null;
        }
        return spawned;
    }
}

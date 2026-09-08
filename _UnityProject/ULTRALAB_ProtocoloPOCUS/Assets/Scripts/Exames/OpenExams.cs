using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class OpenExams : MonoBehaviour, IInteractable
{
    [Header("Exames")]
    [SerializeField] private GameObject panelExam;
    [SerializeField] private GameObject panelConduct;
    [SerializeField] private string exams;
    [SerializeField] private PatientData patientData;

    [Header("Conduta")]
    [SerializeField] private NPC npcConduta;
    [SerializeField] private MedicalData medicalDataUI;

    [Header("Cena Permitida")]
    [SerializeField] private bool tutorialScene = false;

    [Header("Áudio")]
    public EventReference ClickSound;


    // =========================================================
    // ESTADO
    // =========================================================

    private bool conductCompleted = false;

    private ConductState conductState =
        new ConductState();


    public bool ConductCompleted =>
        conductCompleted;

    public PatientData PatientDataReference =>
        patientData;

    public ConductState ConductState =>
        conductState;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (patientData == null)
        {
            Debug.LogError(
                "PatientData não foi atribuído no OpenExams!"
            );

            return;
        }


        // -----------------------------------------------------
        // RESTAURA O ESTADO DO PACIENTE
        // -----------------------------------------------------

        GameSession.LoadPatient(
            patientData
        );


        // -----------------------------------------------------
        // SE O PACIENTE JÁ RECEBEU ALTA,
        // NÃO DEVE APARECER NOVAMENTE
        // -----------------------------------------------------

        if (GameSession.IsPatientDischarged(
            patientData))
        {
            Destroy(gameObject);

            return;
        }


        // -----------------------------------------------------
        // RESTAURA A CONDUTA
        // -----------------------------------------------------

        bool restoredConductCompleted;

        bool hasConduct =
            GameSession.LoadConduct(
                patientData,
                conductState,
                out restoredConductCompleted
            );

        if (hasConduct)
        {
            conductCompleted =
                restoredConductCompleted;
        }
        else
        {
            // Só limpa quando ainda não existe
            // uma conduta salva para esse paciente.
            conductState.Clear();

            conductCompleted = false;
        }


        // -----------------------------------------------------
        // REGISTRA O PACIENTE
        // -----------------------------------------------------

        if (PatientManager.Instance != null)
        {
            PatientManager.Instance.RegisterPatient(
                this
            );
        }
        else
        {
            Debug.LogError(
                "PatientManager não encontrado!"
            );
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (PatientManager.Instance != null)
        {
            PatientManager.Instance.UnregisterPatient(
                this
            );
        }
    }


    // =========================================================
    // INTERAÇÃO
    // =========================================================

    public bool CanInteract()
    {
        return true;
    }


    public void Interact()
    {
        PlayClickSound();

        panelExam.SetActive(true);

        PauseController.SetPause(true);

        CurrentPatient.Data =
            patientData;

        CurrentPatient.Object =
            this;


        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.DisablePlayer();

            if (PlayerReferences.Instance.InteractIcon != null)
            {
                PlayerReferences.Instance.InteractIcon
                    .SetActive(false);
            }
        }
    }


    // =========================================================
    // FECHAR PAINEL
    // =========================================================

    public void ClosePanel()
    {
        PlayClickSound();

        panelExam.SetActive(false);

        PauseController.SetPause(false);

        CurrentPatient.Data = null;
        CurrentPatient.Object = null;


        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.EnablePlayer();

            if (PlayerReferences.Instance.InteractIcon != null)
            {
                PlayerReferences.Instance.InteractIcon
                    .SetActive(true);
            }
        }
    }


    // =========================================================
    // ABRIR EXAMES
    // =========================================================

    public void OpenExam()
    {
        PlayClickSound();


        // Salva o tempo atual antes de mudar de cena
        Timer timer =
            FindFirstObjectByType<Timer>();

        if (timer != null)
        {
            timer.SaveCurrentTime();
        }


        // Salva qual cena devemos retornar
        GameSession.SetOriginalScene(
            SceneManager.GetActiveScene().name
        );


        // Salva o paciente atual
        GameSession.SavePatient(
            patientData
        );


        // Salva a conduta atual
        GameSession.SaveConduct(
            patientData,
            conductState,
            conductCompleted
        );


        // Mantém o paciente atual
        CurrentPatient.Data =
            patientData;

        CurrentPatient.Object =
            this;


        SceneManager.LoadScene(
            exams
        );
    }


    // =========================================================
    // ABRIR CONDUTA
    // =========================================================

    public void OpenConduta()
    {
        PlayClickSound();

        CurrentPatient.Data =
            patientData;

        CurrentPatient.Object =
            this;


        if (medicalDataUI != null)
        {
            // Carrega a conduta deste paciente
            medicalDataUI.LoadState(
                conductState
            );
        }
        else
        {
            Debug.LogError(
                $"MedicalData não foi atribuído " +
                $"no paciente {patientData.patientName}!"
            );
        }


        if (tutorialScene)
        {
            panelExam.SetActive(false);

            npcConduta.OnDialogueEnded =
                ReturnToExamPanel;

            npcConduta.StartDialogueExternally();
        }
        else
        {
            panelExam.SetActive(false);

            panelConduct.SetActive(true);

            PauseController.SetPause(false);
        }
    }


    // =========================================================
    // SALVAR CONDUTA
    // =========================================================

    public void SaveConductState()
    {
        if (medicalDataUI == null)
            return;


        // Pega o estado atual da UI
        conductState =
            medicalDataUI.GetCurrentState();


        // Salva também no GameSession
        GameSession.SaveConduct(
            patientData,
            conductState,
            conductCompleted
        );


        Debug.Log(
            $"[OpenExams] Conduta salva do paciente: " +
            $"{patientData.patientName}"
        );
    }


    // =========================================================
    // CONCLUIR CONDUTA
    // =========================================================

    public void CompleteConduct()
    {
        SaveConductState();

        conductCompleted = true;


        // Salva novamente agora que foi concluída
        GameSession.SaveConduct(
            patientData,
            conductState,
            conductCompleted
        );

    }


    // =========================================================
    // RESETAR NO NOVO DIA
    // =========================================================

    public void ResetConductForNewDay()
    {
        conductState.Clear();

        conductCompleted = false;


        // IMPORTANTE:
        // A conduta do novo dia começa vazia.
        // Isso também atualiza o GameSession.
        GameSession.SaveConduct(
            patientData,
            conductState,
            false
        );


        Debug.Log(
            $"Conduta do paciente " +
            $"{patientData.patientName} " +
            $"foi resetada para o novo dia."
        );
    }


    // =========================================================
    // RETORNAR PARA O PACIENTE
    // =========================================================

    public void ReturnToExamPanel()
    {
        if (PlayerReferences.Instance?.InteractionDetector != null)
        {
            PlayerReferences.Instance.InteractionDetector
                .ForceInteractable(this);
        }
    }


    // =========================================================
    // ÁUDIO
    // =========================================================

    private void PlayClickSound()
    {
        if (!ClickSound.IsNull)
        {
            RuntimeManager.PlayOneShot(
                ClickSound,
                transform.position
            );
        }
    }
}
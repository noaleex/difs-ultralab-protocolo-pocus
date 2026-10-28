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

    /// <summary>Dados clínicos sorteados para o dia atual, sem alterar o asset original.</summary>
    public PatientData ClinicalData
    {
        get
        {
            int day = Timer.Instance != null ? Mathf.Max(1, Timer.Instance.CurrentDay) : 1;
            return PatientDailyState.GetClinicalData(patientData, day);
        }
    }

    public bool TryDeliverFood(ChoiceFood.TipoComida selected, out bool correct, out bool alreadyDelivered)
    {
        int day = Timer.Instance != null ? Mathf.Max(1, Timer.Instance.CurrentDay) : 1;
        bool attempted = PatientDailyState.TryDeliverFood(patientData, day, selected, out correct, out alreadyDelivered);
        if (!attempted)
            return false;

        int welfareBefore = GameSession.GetPatientWelfare(patientData);
        int delta = correct ? 1 : -1;
        int welfareAfter = Mathf.Clamp(welfareBefore + delta, 0, 74);
        GameSession.SetPatientWelfare(patientData, welfareAfter);
        GameSession.SavePatient(patientData);

        Debug.Log(
            $"[Entrega de comida] Paciente: {patientData.patientName} | " +
            $"Resultado: {(correct ? "CORRETA" : "INCORRETA")} | " +
            $"Welfare: {(correct ? "+1" : "-1")} | " +
            $"Valor: {welfareBefore} -> {welfareAfter}",
            this);
        return true;
    }

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
        // Garante que os dados clínicos do dia estejam preparados.
        int currentDay = Timer.Instance != null ? Mathf.Max(1, Timer.Instance.CurrentDay) : 1;
        PatientDailyState.GetClinicalData(patientData, currentDay);

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
        // Não limpamos CurrentPatient aqui: OpenExam pode trocar de cena e
        // a cena de exames ainda pode precisar de CurrentPatient.Data.
        if (PatientManager.Instance != null)
            PatientManager.Instance.UnregisterPatient(this);
    }


    /// <summary>Usado pelo host para ligar o painel instanciado ao NPC atual.</summary>
    public void BindRuntimePanels(GameObject runtimeExamPanel, GameObject sharedConductPanel, MedicalData runtimeMedicalData)
    {
        panelExam = runtimeExamPanel;
        if (sharedConductPanel != null) panelConduct = sharedConductPanel;
        if (runtimeMedicalData != null) medicalDataUI = runtimeMedicalData;
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
        if (patientData == null)
        {
            Debug.LogError("[OpenExams] Não é possível interagir: PatientData não foi atribuído.", this);
            return;
        }

        PlayClickSound();

        CurrentPatient.Select(this);

        bool panelInstantiated = PatientPanelUIHost.Instance != null &&
                                 PatientPanelUIHost.Instance.OpenPanel(this);

        if (!panelInstantiated)
        {
            if (panelExam != null)
                panelExam.SetActive(true);
            else
                Debug.LogError($"[OpenExams] Nenhum PatientPanelUIHost ativo e Panel Exam não foi atribuído para {patientData.patientName}.", this);
        }

        PauseController.SetPause(true);


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

        if (panelExam != null)
            panelExam.SetActive(false);

        if (PatientPanelUIHost.Instance != null)
            PatientPanelUIHost.Instance.ClosePanel(this);

        PauseController.SetPause(false);

        CurrentPatient.ClearIfCurrent(this);


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
        if (patientData == null)
        {
            Debug.LogError("[OpenExams] Não é possível abrir exames sem PatientData.", this);
            return;
        }

        PlayClickSound();

        if (Timer.Instance != null)
        {
            Timer.Instance.SaveCurrentTime();
        }
        else
        {
            Debug.LogWarning("[OpenExams] Timer.Instance não está disponível. O horário não será salvo antes de abrir os exames.", this);
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        
        GameSession.SetOriginalScene(currentSceneName);
        GameSession.SavePatient(patientData);
        GameSession.SaveConduct(patientData, conductState, conductCompleted);

        if (boot_manager.instance != null && PlayerReferences.Instance != null)
        {
            boot_manager.instance.StorePlayerPosition(
                PlayerReferences.Instance.transform.position, 
                currentSceneName
            );
        }

        CurrentPatient.Select(this);

        PauseController.SetPause(false);

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(exams);
        }
        else
        {
            SceneManager.LoadScene(exams);
        }
    }


    // =========================================================
    // ABRIR CONDUTA
    // =========================================================

    public void OpenConduta()
    {
        PlayClickSound();

        if (patientData == null)
        {
            Debug.LogError("[OpenExams] Não é possível abrir a conduta sem PatientData.", this);
            return;
        }

        CurrentPatient.Select(this);


        if (medicalDataUI != null)
        {
            medicalDataUI.patientData = ClinicalData;
            // Cada OpenExams mantém seu próprio ConductState.
            // O formulário compartilhado sempre carrega o estado do NPC selecionado.
            medicalDataUI.LoadState(conductState);
        }
        else
        {
            Debug.LogError(
                $"MedicalData não foi atribuído " +
                $"no paciente {patientData.patientName}!"
            );
            return;
        }


        if (tutorialScene)
        {
            if (panelExam != null)
                panelExam.SetActive(false);

            if (npcConduta == null)
            {
                Debug.LogError("[OpenExams] NPC de conduta não foi atribuído para o tutorial.", this);
                return;
            }

            npcConduta.OnDialogueEnded = ReturnToExamPanel;
            npcConduta.StartDialogueExternally();
        }
        else
        {
            if (panelExam != null)
                panelExam.SetActive(false);

            if (panelConduct != null)
                panelConduct.SetActive(true);
            else
                Debug.LogError($"[OpenExams] Panel Conduct não foi atribuído para {patientData.patientName}.", this);

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
        if (CurrentPatient.Object != this)
        {
            Debug.LogWarning("[OpenExams] Estado não salvo: este NPC não é o paciente atualmente selecionado.", this);
            return;
        }

        conductState = medicalDataUI.GetCurrentState();


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
        if (patientData == null)
        {
            Debug.LogError("[OpenExams] Não é possível concluir uma conduta sem PatientData.", this);
            return;
        }

        if (CurrentPatient.Object != this)
        {
            Debug.LogError("[OpenExams] Tentativa de concluir a conduta de um paciente que não está selecionado.", this);
            return;
        }

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
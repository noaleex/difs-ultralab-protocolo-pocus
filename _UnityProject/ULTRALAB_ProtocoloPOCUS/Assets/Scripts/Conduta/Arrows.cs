using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;

public class Arrows : MonoBehaviour
{
    [Header("Páginas")]
    [SerializeField] private GameObject[] pages;
    [SerializeField] private int currentPageIndex = 0;
    [SerializeField] private GameObject conductPanel;

    [Header("Referências")]
    [SerializeField] private MedicalData medicalDataUI;
    [SerializeField] private PatientConductEvaluator conductEvaluator;

    [Header("Input System")]
    [SerializeField] private InputActionReference upPageAction;
    [SerializeField] private InputActionReference downPageAction;

    [Header("FMOD - Sons")]
    [SerializeField] private EventReference somClick;


    // =========================================================
    // INPUT SYSTEM
    // =========================================================

    private void OnEnable()
    {
        if (upPageAction != null)
        {
            upPageAction.action.performed += OnUpPagePressed;
            upPageAction.action.Enable();
        }

        if (downPageAction != null)
        {
            downPageAction.action.performed += OnDownPagePressed;
            downPageAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (upPageAction != null)
        {
            upPageAction.action.performed -= OnUpPagePressed;
            upPageAction.action.Disable();
        }

        if (downPageAction != null)
        {
            downPageAction.action.performed -= OnDownPagePressed;
            downPageAction.action.Disable();
        }
    }

    private void OnUpPagePressed(InputAction.CallbackContext context)
    {
        UpPage();
    }

    private void OnDownPagePressed(InputAction.CallbackContext context)
    {
        DownPage();
    }


    // =========================================================
    // PÁGINAS
    // =========================================================

    public void UpPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        if (currentPageIndex < 0 ||
            currentPageIndex >= pages.Length)
        {
            currentPageIndex = 0;
        }

        pages[currentPageIndex].SetActive(false);

        currentPageIndex =
            (currentPageIndex + 1) % pages.Length;

        pages[currentPageIndex].SetActive(true);
    }


    public void DownPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        if (currentPageIndex < 0 ||
            currentPageIndex >= pages.Length)
        {
            currentPageIndex = 0;
        }

        pages[currentPageIndex].SetActive(false);

        currentPageIndex =
            (currentPageIndex - 1 + pages.Length) % pages.Length;

        pages[currentPageIndex].SetActive(true);
    }


    // =========================================================
    // CONFIRMAR CONDUTA
    // =========================================================

    public void ConfirmConduct()
    {
        TocarSomClick();

        if (CurrentPatient.Data == null ||
            CurrentPatient.Object == null)
        {
            Debug.LogError(
                "[Arrows] Nenhum paciente selecionado!"
            );

            return;
        }

        Debug.Log(
            $"[Arrows] Confirmando conduta do paciente: " +
            $"{CurrentPatient.Data.patientName}"
        );

        if (CurrentPatient.Object.ConductCompleted)
        {
            Debug.LogWarning(
                $"[Arrows] A conduta do paciente " +
                $"{CurrentPatient.Data.patientName} " +
                $"já foi realizada."
            );

            return;
        }

        if (medicalDataUI == null)
        {
            Debug.LogError(
                "[Arrows] MedicalData não foi atribuído no Arrows!"
            );

            return;
        }

        if (conductEvaluator == null)
        {
            Debug.LogError(
                "[Arrows] PatientConductEvaluator " +
                "não foi atribuído no Arrows!"
            );

            return;
        }

        if (!medicalDataUI.ValidateAllFields())
        {
            Debug.LogWarning(
                "[Arrows] Por favor, preencha todos os campos " +
                "antes de confirmar!"
            );

            return;
        }

        int welfareAntes =
            GameSession.GetPatientWelfare(
                CurrentPatient.Data
            );

        Debug.Log(
            $"[Arrows] Welfare antes da conduta: " +
            $"{welfareAntes}"
        );

        bool condutaCorreta =
            conductEvaluator.EvaluatePatient(
                CurrentPatient.Data,
                medicalDataUI
            );

        int welfareDepois =
            GameSession.GetPatientWelfare(
                CurrentPatient.Data
            );

        CurrentPatient.Object.CompleteConduct();

        Debug.Log(
            $"[Arrows] Conduta de " +
            $"{CurrentPatient.Data.patientName} " +
            $"concluída."
        );

        Debug.Log(
            $"[Arrows] Welfare final do paciente " +
            $"{CurrentPatient.Data.patientName}: " +
            $"{GameSession.GetPatientWelfare(CurrentPatient.Data)}"
        );
    }


    // =========================================================
    // FECHAR CONDUTA
    // =========================================================

    public void CloseConduct()
    {
        TocarSomClick();

        if (CurrentPatient.Object != null)
        {
            CurrentPatient.Object.SaveConductState();
        }

        if (medicalDataUI != null)
        {
            medicalDataUI.ResetForm();
        }

        if (conductPanel != null)
        {
            conductPanel.SetActive(false);
        }

        PauseController.SetPause(false);

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.EnablePlayer();
        }

        currentPageIndex = 0;

        CurrentPatient.Data = null;
        CurrentPatient.Object = null;
    }


    // =========================================================
    // SOM
    // =========================================================

    private void TocarSomClick()
    {
        if (!somClick.IsNull)
        {
            RuntimeManager.PlayOneShot(somClick);
        }
    }
}
using System.Collections;
using TMPro;
using UnityEngine;
using FMODUnity;

/// <summary>Entrega a comida selecionada ao paciente atual e gerencia o feedback temporário.</summary>
public sealed class FoodDeliveryButton : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ChoiceFood choiceFood;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text requiredFoodText;
    [SerializeField] private GameObject panelExam;

    [Header("Áudio")]
    public EventReference ClickSound;

    [Header("Feedback")]
    [SerializeField, Min(0.1f)] private float feedbackDuration = 3f;

    private Coroutine feedbackCoroutine;

    private void OnEnable()
    {
        CurrentPatient.Changed += RefreshRequiredFood;
        RefreshRequiredFood();
    }

    private void OnDisable()
    {
        CurrentPatient.Changed -= RefreshRequiredFood;
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }
    }

    public void RefreshRequiredFood()
    {
        if (requiredFoodText == null) return;
        if (CurrentPatient.Data == null)
        {
            requiredFoodText.text = "Nenhum paciente selecionado";
            return;
        }

        int day = Timer.Instance != null ? Mathf.Max(1, Timer.Instance.CurrentDay) : 1;
        PatientData clinical = PatientDailyState.GetClinicalData(CurrentPatient.Data, day);
        requiredFoodText.text = clinical == null
            ? "Alimentação indisponível"
            : "Alimentação prescrita: " + FoodName(clinical.food);
    }

    private static string FoodName(int food)
    {
        switch ((ChoiceFood.TipoComida)food)
        {
            case ChoiceFood.TipoComida.Solido: return "Sólida";
            case ChoiceFood.TipoComida.Liquido: return "Líquida";
            case ChoiceFood.TipoComida.NPT: return "NPT";
            default: return "Não definida";
        }
    }

    /// <summary>Vincule este método ao OnClick do botão Entregar Comida.</summary>
    public void DeliverFood()
    {
        ChoiceFood.TipoComida selected = choiceFood != null
            ? choiceFood.ComidaSelecionada
            : ChoiceFood.TipoComida.Nenhuma;

        // O painel fecha sempre que o botão é acionado. A comida selecionada
        // é consumida após capturar seu tipo, mesmo que a tentativa falhe.
        if (choiceFood != null)
        {
            choiceFood.FecharPainel();
            if (selected != ChoiceFood.TipoComida.Nenhuma)
                choiceFood.ConsumirComida();
        }

        if (choiceFood == null)
        {
            ShowMessage("Selecione uma comida antes de entregar.");
            Debug.LogWarning("[FoodDeliveryButton] ChoiceFood não foi atribuído.", this);
            return;
        }

        if (selected == ChoiceFood.TipoComida.Nenhuma)
        {
            ShowMessage("Você não tinha uma comida selecionada para entregar.");
            Debug.LogWarning("[Entrega de comida] Nenhuma comida estava selecionada.", this);
            return;
        }

        if (CurrentPatient.Object == null || CurrentPatient.Data == null)
        {
            ShowMessage("Interaja com um paciente antes de entregar a comida.");
            Debug.LogWarning("[Entrega de comida] Não há paciente selecionado. A comida selecionada foi consumida.", this);
            return;
        }

        bool correct;
        bool alreadyDelivered;
        bool attempted = CurrentPatient.Object.TryDeliverFood(selected, out correct, out alreadyDelivered);

        if (!attempted && alreadyDelivered)
        {
            ShowMessage("Este paciente já recebeu comida hoje.");
            Debug.LogWarning($"[Entrega de comida] {CurrentPatient.Data.patientName} já recebeu comida hoje. Nenhuma alteração de welfare.", this);
            return;
        }

        if (!attempted)
        {
            ShowMessage("Não foi possível entregar a comida.");
            Debug.LogWarning("[Entrega de comida] A tentativa não foi processada.", this);
            return;
        }

        ShowMessage(correct
            ? $"Comida correta entregue a {CurrentPatient.Data.patientName}! Welfare +1."
            : $"Comida incorreta para {CurrentPatient.Data.patientName}. Welfare -1.");

        ClosePanel();
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message, this);
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay());
    }

    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSecondsRealtime(feedbackDuration);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        feedbackCoroutine = null;
    }

    public void ClosePanel()
    {
        PlayClickSound();

        if (panelExam != null)
            panelExam.SetActive(false);

        PauseController.SetPause(false);

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.EnablePlayer();

            if (PlayerReferences.Instance.InteractIcon != null)
                PlayerReferences.Instance.InteractIcon.SetActive(true);
        }
    }

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

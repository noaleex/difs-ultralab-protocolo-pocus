using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public sealed class FoodDeliveryButton : MonoBehaviour
{
    [Header("Referências configuradas pelo host")]
    [SerializeField] private ChoiceFood choiceFood;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text requiredFoodText;
    [SerializeField] private GameObject panelExam;

    [Header("Áudio")]
    public EventReference ClickSound;

    [Header("Feedback")]
    [SerializeField, Min(0.1f)]
    private float feedbackDuration = 3f;

    private Button button;
    private Coroutine feedbackCoroutine;
    private bool initialized;

    public void Initialize(
        ChoiceFood food,
        TMP_Text feedback,
        TMP_Text requiredFood,
        GameObject runtimePanel)
    {
        choiceFood = food;
        feedbackText = feedback;
        requiredFoodText = requiredFood;
        panelExam = runtimePanel;

        button = GetComponent<Button>();

        if (button == null)
            button = GetComponentInChildren<Button>(true);

        if (button == null)
        {
            Debug.LogError(
                "[FoodDeliveryButton] Não foi encontrado um componente Button.",
                this
            );
            return;
        }

        button.onClick.RemoveListener(DeliverFood);
        button.onClick.AddListener(DeliverFood);

        initialized = true;
        RefreshRequiredFood();
    }

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

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(DeliverFood);
    }

    public void RefreshRequiredFood()
    {
        if (requiredFoodText == null)
            return;

        if (CurrentPatient.Data == null)
        {
            requiredFoodText.text = "Nenhum paciente selecionado";
            return;
        }

        int day = Timer.Instance != null
            ? Mathf.Max(1, Timer.Instance.CurrentDay)
            : 1;

        PatientData clinical =
            PatientDailyState.GetClinicalData(CurrentPatient.Data, day);

        requiredFoodText.text = clinical == null
            ? "Alimentação indisponível"
            : FoodName(clinical.food);
    }

    private static string FoodName(int food)
    {
        switch ((ChoiceFood.TipoComida)food)
        {
            case ChoiceFood.TipoComida.Solido:
                return "Sólida";

            case ChoiceFood.TipoComida.Liquido:
                return "Líquida";

            case ChoiceFood.TipoComida.NPT:
                return "NPT";

            default:
                return "Não definida";
        }
    }

    public void DeliverFood()
    {
        if (!initialized)
        {
            Debug.LogError(
                "[FoodDeliveryButton] O botão ainda não foi inicializado pelo host.",
                this
            );
            return;
        }

        PlayClickSound();

        ChoiceFood.TipoComida selected = choiceFood != null
            ? choiceFood.ComidaSelecionada
            : ChoiceFood.TipoComida.Nenhuma;

        if (choiceFood != null)
        {
            choiceFood.FecharPainel();

            if (selected != ChoiceFood.TipoComida.Nenhuma)
                choiceFood.ConsumirComida();
        }

        if (choiceFood == null)
        {
            ShowMessage("Sistema de escolha de comida indisponível.");
            return;
        }

        if (selected == ChoiceFood.TipoComida.Nenhuma)
        {
            ShowMessage("Selecione uma comida antes de entregar.");
            return;
        }

        if (CurrentPatient.Object == null || CurrentPatient.Data == null)
        {
            ShowMessage("Interaja com um paciente antes de entregar.");
            return;
        }

        bool correct;
        bool alreadyDelivered;

        bool attempted = CurrentPatient.Object.TryDeliverFood(
            selected,
            out correct,
            out alreadyDelivered
        );

        if (!attempted && alreadyDelivered)
        {
            ShowMessage("Este paciente já recebeu comida hoje.");
            return;
        }

        if (!attempted)
        {
            ShowMessage("Não foi possível entregar a comida.");
            return;
        }

        ShowMessage(correct
            ? $"Comida correta entregue a {CurrentPatient.Data.patientName}! Welfare +1."
            : $"Comida incorreta para {CurrentPatient.Data.patientName}. Welfare -1.");

        if (CurrentPatient.Object != null)
            CurrentPatient.Object.ClosePanel();
        else if (panelExam != null)
            panelExam.SetActive(false);
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message, this);

        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay());
    }

    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSecondsRealtime(feedbackDuration);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);

        feedbackCoroutine = null;
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

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMODUnity;

public sealed class FoodDeliveryButton : MonoBehaviour
{
    [Header("Referências configuradas pelo host")]
    [SerializeField] private ChoiceFood choiceFood;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text requiredFoodText;
    [SerializeField] private GameObject panelExam;
    [SerializeField] private Image bgFeedbackImage;

    [Header("Áudio")]
    public EventReference ClickSound;

    [Header("Fade do feedback")]
    [SerializeField, Min(0.01f)] private float fadeInDuration = 0.25f;
    [SerializeField, Min(0.1f)] private float messageDuration = 2f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.35f;

    private Button button;
    private FoodFeedbackCoroutineRunner feedbackRunner;

    private bool initialized;
    private Color feedbackOriginalColor = Color.white;
    private bool deliveryInProgress;

    public void Initialize(
       ChoiceFood food,
       Image bgImage,
       TMP_Text feedback,
       TMP_Text requiredFood,
       GameObject runtimePanel)
    {
        choiceFood = food;
        bgFeedbackImage = bgImage;
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

        if (feedbackText != null)
        {
            feedbackOriginalColor = feedbackText.color;

            Color color = feedbackOriginalColor;
            color.a = 0f;

            feedbackText.color = color;
            feedbackText.gameObject.SetActive(false);

            InitializeFeedbackRunner();
        }

        if (bgFeedbackImage != null)
        {
            Color color = bgFeedbackImage.color;
            color.a = 0f;

            bgFeedbackImage.color = color;
            bgFeedbackImage.gameObject.SetActive(false);
        }

        initialized = true;

        RefreshRequiredFood();
        RefreshDeliveryButton();
    }

    private void OnEnable()
    {
        CurrentPatient.Changed += OnPatientChanged;
        GameSession.DayChanged += OnDayChanged;

        RefreshRequiredFood();
        RefreshDeliveryButton();
    }

    private void OnDisable()
    {
        CurrentPatient.Changed -= OnPatientChanged;
        GameSession.DayChanged -= OnDayChanged;

        // Não interrompemos o feedback aqui.
        // A Coroutine pertence ao Canvas, não ao botão.
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(DeliverFood);
    }

    // =====================================================
    // FEEDBACK
    // =====================================================

    private void InitializeFeedbackRunner()
    {
        if (feedbackText == null)
            return;

        Transform canvasRoot = feedbackText.transform.root;

        if (canvasRoot == null)
            return;

        // O feedback precisa estar fora do painel que será fechado.
        if (panelExam != null &&
            feedbackText.transform.IsChildOf(panelExam.transform))
        {
            Debug.LogError(
                "[FoodDeliveryButton] O FeedbackText está dentro do " +
                "panelExam. Coloque-o como objeto separado no Canvas.",
                feedbackText
            );

            return;
        }

        feedbackRunner =
            canvasRoot.GetComponent<FoodFeedbackCoroutineRunner>();

        if (feedbackRunner == null)
        {
            feedbackRunner =
                canvasRoot.gameObject.AddComponent<FoodFeedbackCoroutineRunner>();
        }
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message, this);

        if (feedbackText == null)
        {
            Debug.LogError(
                "[FoodDeliveryButton] FeedbackText não foi atribuído.",
                this
            );

            return;
        }

        if (feedbackRunner == null)
            InitializeFeedbackRunner();

        if (feedbackRunner == null)
        {
            Debug.LogError(
                "[FoodDeliveryButton] Não foi possível inicializar " +
                "o controlador de feedback.",
                this
            );

            return;
        }

        feedbackRunner.ShowMessage(
    feedbackText,
    bgFeedbackImage,
    message,
    feedbackOriginalColor,
    fadeInDuration,
    messageDuration,
    fadeOutDuration
);
    }

    // =====================================================
    // PACIENTE E DIA
    // =====================================================

    private void OnPatientChanged()
    {
        RefreshRequiredFood();
        RefreshDeliveryButton();
    }

    private void OnDayChanged(int day)
    {
        RefreshRequiredFood();
        RefreshDeliveryButton();
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

        requiredFoodText.text =
            clinical == null
                ? "Alimentação indisponível"
                : FoodName(clinical.food);
    }

    private void RefreshDeliveryButton()
    {
        if (button == null || !initialized)
            return;

        bool hasPatient = CurrentPatient.Data != null;

        int day = Timer.Instance != null
            ? Mathf.Max(1, Timer.Instance.CurrentDay)
            : 1;

        bool alreadyDelivered =
            hasPatient &&
            PatientDailyState.HasDeliveredFood(CurrentPatient.Data, day);

        button.interactable = hasPatient && !alreadyDelivered;
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

    // =====================================================
    // ENTREGA DE COMIDA
    // =====================================================

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

        if (deliveryInProgress)
            return;

        deliveryInProgress = true;

        if (CurrentPatient.Object == null || CurrentPatient.Data == null)
        {
            deliveryInProgress = false;
            ShowMessage("Interaja com um paciente antes de entregar.");
            return;
        }

        int day = Timer.Instance != null
            ? Mathf.Max(1, Timer.Instance.CurrentDay)
            : 1;

        if (PatientDailyState.HasDeliveredFood(CurrentPatient.Data, day))
        {
            RefreshDeliveryButton();
            return;
        }

        if (choiceFood == null)
        {
            ShowMessage("Sistema de escolha de comida indisponível.");
            return;
        }

        ChoiceFood.TipoComida selected =
            choiceFood.ComidaSelecionada;

        if (selected == ChoiceFood.TipoComida.Nenhuma)
        {
            ShowMessage("Selecione uma comida.");
            return;
        }

        PlayClickSound();

        choiceFood.FecharPainel();

        bool correct;
        bool alreadyDelivered;

        bool attempted =
            CurrentPatient.Object.TryDeliverFood(
                selected,
                out correct,
                out alreadyDelivered
            );

        if (!attempted)
        {
            if (alreadyDelivered)
            {
                RefreshDeliveryButton();
            }
            else
            {
                ShowMessage("Não foi possível entregar a comida.");
            }

            return;
        }

        choiceFood.ConsumirComida();

        // Bloqueia novas entregas.
        if (button != null)
            button.interactable = false;

        // Mostra o feedback no Canvas, fora do painel do paciente.
        ShowMessage(
            correct
                ? "Paciente alimentado, opção correta."
                : "Paciente alimentado, opção errada."
        );

        // Fecha o painel sem interromper o feedback.
        if (CurrentPatient.Object != null)
        {
            CurrentPatient.Object.ClosePanel();
        }
        else if (panelExam != null)
        {
            panelExam.SetActive(false);
        }
    }

    // =====================================================
    // ÁUDIO
    // =====================================================

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


// =========================================================
// CONTROLADOR INDEPENDENTE DO FEEDBACK
// =========================================================


public sealed class FoodFeedbackCoroutineRunner : MonoBehaviour
{
    private Coroutine currentCoroutine;

    public void ShowMessage(
        TMP_Text text,
        Image background,
        string message,
        Color originalColor,
        float fadeInDuration,
        float messageDuration,
        float fadeOutDuration)
    {
        if (text == null)
            return;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        currentCoroutine = StartCoroutine(
            PlayFeedback(
                text,
                background,
                message,
                originalColor,
                fadeInDuration,
                messageDuration,
                fadeOutDuration
            )
        );
    }

    private System.Collections.IEnumerator PlayFeedback(
        TMP_Text text,
        Image background,
        string message,
        Color originalColor,
        float fadeInDuration,
        float messageDuration,
        float fadeOutDuration)
    {
        text.text = message;

        text.gameObject.SetActive(true);

        if (background != null)
            background.gameObject.SetActive(true);

        SetAlpha(text, originalColor, 0f);

        if (background != null)
            SetBackgroundAlpha(background, 0f);

        // FADE IN
        yield return Fade(
            text,
            originalColor,
            0f,
            1f,
            fadeInDuration,
            background
        );

        // TEMPO VISÍVEL
        yield return new WaitForSecondsRealtime(messageDuration);

        // FADE OUT
        yield return Fade(
            text,
            originalColor,
            1f,
            0f,
            fadeOutDuration,
            background
        );

        // DESATIVA AMBOS
        if (text != null)
        {
            SetAlpha(text, originalColor, 0f);
            text.gameObject.SetActive(false);
        }

        if (background != null)
        {
            SetBackgroundAlpha(background, 0f);
            background.gameObject.SetActive(false);
        }

        currentCoroutine = null;
    }

    private System.Collections.IEnumerator Fade(
        TMP_Text text,
        Color originalColor,
        float from,
        float to,
        float duration,
        Image background)
    {
        if (duration <= 0f)
        {
            SetAlpha(text, originalColor, to);

            if (background != null)
                SetBackgroundAlpha(background, to);

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (text == null)
                yield break;

            elapsed += Time.unscaledDeltaTime;

            float alpha = Mathf.Lerp(
                from,
                to,
                Mathf.Clamp01(elapsed / duration)
            );

            SetAlpha(text, originalColor, alpha);

            if (background != null)
                SetBackgroundAlpha(background, alpha);

            yield return null;
        }

        SetAlpha(text, originalColor, to);

        if (background != null)
            SetBackgroundAlpha(background, to);
    }

    private void SetAlpha(
        TMP_Text text,
        Color originalColor,
        float alpha)
    {
        if (text == null)
            return;

        Color color = originalColor;
        color.a = alpha;

        text.color = color;
    }

    private void SetBackgroundAlpha(
        Image background,
        float alpha)
    {
        if (background == null)
            return;

        Color color = background.color;
        color.a = alpha;

        background.color = color;
    }
}


using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeveloperWelfareController : MonoBehaviour
{
    [Header("Feedback na tela")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float messageDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Configurações")]
    [SerializeField] private int welfareChange = 10;

    private InputAction addPointsAction;
    private InputAction removePointsAction;
    private InputAction skipDayAction;

    private CanvasGroup feedbackCanvasGroup;
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        // F7: acrescentar pontos
        addPointsAction = new InputAction(
            "DeveloperAddWelfare",
            InputActionType.Button
        );

        addPointsAction.AddBinding("<Keyboard>/f7");
        addPointsAction.performed += OnAddPoints;

        // F8: diminuir pontos
        removePointsAction = new InputAction(
            "DeveloperRemoveWelfare",
            InputActionType.Button
        );

        removePointsAction.AddBinding("<Keyboard>/f8");
        removePointsAction.performed += OnRemovePoints;

        // F9: pular dia
        skipDayAction = new InputAction(
            "DeveloperSkipDay",
            InputActionType.Button
        );

        skipDayAction.AddBinding("<Keyboard>/f9");
        skipDayAction.performed += OnSkipDay;

        // Configurar feedback
        if (feedbackText != null)
        {
            feedbackCanvasGroup =
                feedbackText.GetComponent<CanvasGroup>();

            if (feedbackCanvasGroup == null)
            {
                feedbackCanvasGroup =
                    feedbackText.gameObject.AddComponent<CanvasGroup>();
            }

            feedbackCanvasGroup.alpha = 0f;
            feedbackText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        addPointsAction.Enable();
        removePointsAction.Enable();
        skipDayAction.Enable();
    }

    private void OnDisable()
    {
        addPointsAction.Disable();
        removePointsAction.Disable();
        skipDayAction.Disable();

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        addPointsAction.performed -= OnAddPoints;
        removePointsAction.performed -= OnRemovePoints;
        skipDayAction.performed -= OnSkipDay;

        addPointsAction.Dispose();
        removePointsAction.Dispose();
        skipDayAction.Dispose();
    }

    // =====================================================
    // INPUT SYSTEM
    // =====================================================

    private void OnAddPoints(InputAction.CallbackContext context)
    {
        ChangeAllPatients(welfareChange);
    }

    private void OnRemovePoints(InputAction.CallbackContext context)
    {
        ChangeAllPatients(-welfareChange);
    }

    // =====================================================
    // ALTERAR WELFARE DOS PACIENTES
    // =====================================================

    private void ChangeAllPatients(int amount)
    {
        if (PatientManager.Instance == null)
        {
            Debug.LogWarning(
                "[DeveloperWelfare] PatientManager não encontrado."
            );

            return;
        }

        IReadOnlyList<OpenExams> patients =
            PatientManager.Instance.GetPatients();

        // Guardar os PatientData antes de modificar o Welfare.
        // Isso evita problemas caso a progressão destrua algum NPC.
        HashSet<PatientData> uniquePatients =
            new HashSet<PatientData>();

        foreach (OpenExams patient in patients)
        {
            if (patient == null)
                continue;

            PatientData data = patient.PatientDataReference;

            if (data != null)
                uniquePatients.Add(data);
        }

        if (uniquePatients.Count == 0)
        {
            Debug.LogWarning(
                "[DeveloperWelfare] Nenhum paciente registrado na cena."
            );

            ShowFeedback("Nenhum paciente encontrado na cena.");
            return;
        }

        foreach (PatientData patientData in uniquePatients)
        {
            int previous =
                GameSession.GetPatientWelfare(patientData);

            int newValue = Mathf.Clamp(
                previous + amount,
                0,
                74
            );

            if (newValue == previous)
                continue;

            // Utiliza o sistema oficial de Welfare do projeto.
            GameSession.SetPatientWelfare(
                patientData,
                newValue
            );

            Debug.Log(
                $"[DeveloperWelfare] {patientData.patientName}: " +
                $"{previous} -> {newValue}"
            );
        }

        string message = amount > 0
            ? "+10 pontos para todos os pacientes"
            : "-10 pontos para todos os pacientes";

        ShowFeedback(message);
    }

    // =====================================================
    // FEEDBACK NA TELA
    // =====================================================

    private void ShowFeedback(string message)
    {
        if (feedbackText == null || feedbackCanvasGroup == null)
            return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        feedbackCoroutine = StartCoroutine(FeedbackRoutine());
    }

    private IEnumerator FeedbackRoutine()
    {
        yield return Fade(0f, 1f, fadeInDuration);

        yield return new WaitForSecondsRealtime(messageDuration);

        yield return Fade(1f, 0f, fadeOutDuration);

        feedbackText.gameObject.SetActive(false);
        feedbackCoroutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            feedbackCanvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            feedbackCanvasGroup.alpha = Mathf.Lerp(
                from,
                to,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        feedbackCanvasGroup.alpha = to;
    }

    private void OnSkipDay(InputAction.CallbackContext context)
    {
        if (Timer.Instance == null)
        {
            Debug.LogError(
                "[Developer] Timer não encontrado."
            );

            return;
        }

        Debug.Log("[Developer] Solicitando avanço de dia.");

        Timer.Instance.DeveloperSkipToNextDay();
    }
}

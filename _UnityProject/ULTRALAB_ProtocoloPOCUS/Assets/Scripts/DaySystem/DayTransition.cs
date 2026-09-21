using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayTransition : MonoBehaviour
{
    public static DayTransition Instance;

    [Header("UI")]
    [SerializeField] private Image blackPanel;
    [SerializeField] private TextMeshProUGUI dayText;

    [Header("Configuração da Transição")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float textTime = 1.5f;

    [Header("Verificação dos Pacientes")]
    [SerializeField] private PatientDayValidator patientDayValidator;


    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "Existe mais de um DayTransition na cena."
            );
        }

        Instance = this;

        ResetVisualState();
    }


    // =====================================================
    // DESTROY
    // =====================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =====================================================
    // RESETAR ESTADO VISUAL
    // =====================================================

    private void ResetVisualState()
    {
        if (blackPanel != null)
        {
            Color color = blackPanel.color;
            color.a = 0f;
            blackPanel.color = color;
        }

        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }
    }


    // =====================================================
    // COMEÇAR DIA
    // =====================================================

    public void BeginDay(
        int day,
        Action onFinished)
    {
        StartCoroutine(
            DayRoutine(
                day,
                onFinished
            )
        );
    }


    private IEnumerator DayRoutine(
        int day,
        Action onFinished)
    {
        PauseController.SetPause(true);

        // Garantir que começa transparente
        if (blackPanel != null)
        {
            Color color = blackPanel.color;
            color.a = 0f;
            blackPanel.color = color;
        }

        // Fade para preto
        yield return Fade(1f);

        // Mostrar dia
        if (dayText != null)
        {
            dayText.text = $"Dia {day}. . .";
            dayText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(textTime);

        // Voltar para o jogo
        yield return Fade(0f);

        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        PauseController.SetPause(false);

        onFinished?.Invoke();
    }


    // =====================================================
    // FIM NATURAL DO DIA
    // =====================================================
    //
    // Este método é chamado quando o relógio chega
    // naturalmente ao endHour.
    //
    // NÃO exige que todos os pacientes tenham conduta.
    //
    // Pacientes incompletos serão avaliados como erros
    // pelo PatientDayValidator.
    // =====================================================

    public void CheckPatientsAtEndOfDay(
        Action onFinished)
    {
        StartCoroutine(
            CheckPatientsRoutine(
                onFinished
            )
        );
    }


    private IEnumerator CheckPatientsRoutine(
        Action onFinished)
    {
        PauseController.SetPause(true);

        if (patientDayValidator == null)
        {
            Debug.LogError(
                "PatientDayValidator não foi atribuído no DayTransition!"
            );

            PauseController.SetPause(false);

            yield break;
        }

        bool validationFinished = false;
        bool validationSuccess = false;

        // =================================================
        // VALIDAR PACIENTES
        // =================================================

        yield return StartCoroutine(
            patientDayValidator.ValidatePatientsAtEndOfDay(
                result =>
                {
                    validationSuccess = result;
                    validationFinished = true;
                },
                dayText
            )
        );

        // =================================================
        // GARANTIR QUE A VALIDAÇÃO TERMINOU
        // =================================================

        if (!validationFinished)
        {
            validationFinished = true;
        }

        // =================================================
        // NÃO AVANÇAR
        // =================================================
        //
        // Normalmente isso poderá acontecer em situações
        // como Game Over.
        // =================================================

        if (!validationSuccess)
        {
            PauseController.SetPause(false);
            yield break;
        }

        // =================================================
        // RESETAR PACIENTES
        // =================================================

        patientDayValidator.ResetAllPatientsForNewDay();

        // =================================================
        // TRANSIÇÃO
        // =================================================

        yield return Fade(1f);

        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        yield return Fade(0f);

        PauseController.SetPause(false);

        onFinished?.Invoke();
    }


    // =====================================================
    // SKIP DAY
    // =====================================================
    //
    // Diferente do fim natural:
    //
    // O SkipDay SÓ pode funcionar se todos os pacientes
    // tiverem a conduta preenchida.
    // =====================================================

    public void CheckPatientsForSkipDay(
        Action<bool> onFinished)
    {
        StartCoroutine(
            CheckPatientsForSkipDayRoutine(
                onFinished
            )
        );
    }


    private IEnumerator CheckPatientsForSkipDayRoutine(
    Action<bool> onFinished)
    {
        PauseController.SetPause(true);

        if (patientDayValidator == null)
        {
            Debug.LogError(
                "PatientDayValidator não foi atribuído no DayTransition."
            );

            PauseController.SetPause(false);

            onFinished?.Invoke(false);

            yield break;
        }

        bool validationFinished = false;
        bool validationSuccess = false;

        // =================================================
        // VALIDAR PACIENTES PARA SKIP
        // =================================================

        yield return StartCoroutine(
            patientDayValidator.ValidatePatientsForSkipDay(
                result =>
                {
                    validationSuccess = result;
                    validationFinished = true;
                },
                dayText
            )
        );

        // =================================================
        // VERIFICAR RESULTADO
        // =================================================

        if (!validationFinished)
        {
            Debug.LogWarning(
                "A validação dos pacientes terminou sem retornar resultado."
            );

            PauseController.SetPause(false);

            onFinished?.Invoke(false);

            yield break;
        }

        // =================================================
        // NÃO PODE PULAR O DIA
        // =================================================

        if (!validationSuccess)
        {
            PauseController.SetPause(false);

            onFinished?.Invoke(false);

            yield break;
        }

        // =================================================
        // RESETAR PACIENTES
        // =================================================

        patientDayValidator.ResetAllPatientsForNewDay();

        // =================================================
        // TRANSIÇÃO
        // =================================================

        yield return Fade(1f);

        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        yield return new WaitForSecondsRealtime(textTime);

        yield return Fade(0f);

        PauseController.SetPause(false);

        onFinished?.Invoke(true);
    }


    // =====================================================
    // FADE
    // =====================================================

    private IEnumerator Fade(float target)
    {
        if (blackPanel == null)
        {
            Debug.LogError(
                "Black Panel não foi atribuído!"
            );

            yield break;
        }

        float start = blackPanel.color.a;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;

            Color color = blackPanel.color;

            color.a = Mathf.Lerp(
                start,
                target,
                t / fadeTime
            );

            blackPanel.color = color;

            yield return null;
        }

        Color finalColor = blackPanel.color;

        finalColor.a = target;

        blackPanel.color = finalColor;
    }
}
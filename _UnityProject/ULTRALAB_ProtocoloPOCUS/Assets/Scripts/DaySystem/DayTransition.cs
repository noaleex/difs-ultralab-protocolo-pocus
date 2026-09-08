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
        if (Instance != null &&
            Instance != this)
        {
            Debug.LogWarning(
                "Existe mais de um DayTransition na cena."
            );
        }


        Instance = this;
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
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


        yield return Fade(1);


        if (dayText != null)
        {
            dayText.text =
                $"Dia {day}. . .";


            dayText.gameObject
                .SetActive(true);
        }


        yield return
            new WaitForSecondsRealtime(
                textTime
            );


        yield return Fade(0);


        if (dayText != null)
        {
            dayText.gameObject
                .SetActive(false);
        }


        PauseController.SetPause(false);


        onFinished?.Invoke();
    }


    // =====================================================
    // VERIFICAR FIM DO DIA
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


        bool finished = false;


        // =================================================
        // VALIDAR PACIENTES
        // =================================================

        yield return StartCoroutine(
            patientDayValidator
                .ValidatePatientsAtEndOfDay(
                    result =>
                    {
                        finished = result;
                    },
                    dayText
                )
        );


        // =================================================
        // NÃO PODE TERMINAR
        // =================================================

        if (!finished)
        {
            PauseController.SetPause(false);

            yield break;
        }


        // =================================================
        // NOVO DIA
        // =================================================
        //
        // Somente as condutas são resetadas.
        //
        // welfareScore continua salvo.
        // =================================================

        patientDayValidator
            .ResetAllPatientsForNewDay();


        // =================================================
        // TRANSIÇÃO
        // =================================================

        yield return Fade(1);


        if (dayText != null)
        {
            dayText.gameObject
                .SetActive(false);
        }


        yield return Fade(0);


        PauseController.SetPause(false);


        onFinished?.Invoke();
    }


    // =====================================================
    // FADE
    // =====================================================

    private IEnumerator Fade(
        float target)
    {
        if (blackPanel == null)
        {
            Debug.LogError(
                "Black Panel não foi atribuído!"
            );

            yield break;
        }


        float start =
            blackPanel.color.a;


        float t = 0f;


        while (t < fadeTime)
        {
            t +=
                Time.unscaledDeltaTime;


            Color color =
                blackPanel.color;


            color.a =
                Mathf.Lerp(
                    start,
                    target,
                    t / fadeTime
                );


            blackPanel.color =
                color;


            yield return null;
        }


        Color finalColor =
            blackPanel.color;


        finalColor.a =
            target;


        blackPanel.color =
            finalColor;
    }
}
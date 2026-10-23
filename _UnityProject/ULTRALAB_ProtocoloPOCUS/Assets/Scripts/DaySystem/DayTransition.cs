using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayTransition : MonoBehaviour
{
    public static DayTransition Instance;

    [Header("UI")]
    [SerializeField] private RawImage blackPanel; // <-- Atualizado para RawImage
    [SerializeField] private TextMeshProUGUI dayText;

    [Header("Configuração da Transição")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float textTime = 2.5f;

    [Header("Verificação dos Pacientes")]
    [SerializeField] private PatientDayValidator patientDayValidator;

    private void Awake()
    {
        Instance = this;
        
        // Garante que os elementos visuais comecem desligados
        if (blackPanel != null) blackPanel.gameObject.SetActive(false);
        if (dayText != null) dayText.gameObject.SetActive(false);
    }

    public void BeginDay(int day, Action onFinished, bool instantBlack = false, string nota = "")
    {
        StartCoroutine(DayRoutine(day, onFinished, instantBlack, nota));
    }

    private IEnumerator DayRoutine(int day, Action onFinished, bool instantBlack, string nota)
    {
        pause_manager.SetPause(true);

        // Bloqueia qualquer ação ou movimento residual do player
        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.DisablePlayer();
        }

        if (blackPanel != null)
        {
            blackPanel.gameObject.SetActive(true);
        }

        if (instantBlack && blackPanel != null)
        {
            Color c = blackPanel.color;
            c.a = 1f;
            blackPanel.color = c;
            yield return new WaitForSecondsRealtime(0.6f); 
        }
        else
        {
            yield return Fade(1);
        }

        if (dayText != null)
        {
            // Formata o texto base do dia
            string textoDia = $"Plantão de Integração\n<size=75%>Dia {day}</size>";
            
            // Se houver uma nota (vindo do botão de dormir), adiciona no final em amarelo
            if (!string.IsNullOrEmpty(nota))
            {
                string corNota = (nota == "D" || nota == "N/A") ? "red" : "yellow";
                textoDia += $"\n\n<size=60%><color={corNota}>Desempenho: {nota}</color></size>";
            }

            dayText.text = textoDia;
            dayText.gameObject.SetActive(true);
        }

        // Espera o tempo de leitura do texto
        yield return new WaitForSecondsRealtime(textTime);

        // SOME O TEXTO PRIMEIRO
        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        // Espera um tempinho extra só com a tela preta
        yield return new WaitForSecondsRealtime(0.5f);

        // AGORA CLAREA A TELA
        yield return Fade(0);

        if (blackPanel != null)
        {
            blackPanel.gameObject.SetActive(false);
        }

        pause_manager.SetPause(false);

        // Devolve o controle ao jogador
        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.EnablePlayer();
        }

        onFinished?.Invoke();
    }

    public void CheckPatientsAtEndOfDay(Action onFinished)
    {
        StartCoroutine(CheckPatientsRoutine(onFinished));
    }

    private IEnumerator CheckPatientsRoutine(Action onFinished)
    {
        pause_manager.SetPause(true);

        if (patientDayValidator == null)
        {
            Debug.LogError("PatientDayValidator não foi atribuído!");
            pause_manager.SetPause(false);
            yield break;
        }

        bool finished = false;

        yield return StartCoroutine(
            patientDayValidator.ValidatePatientsAtEndOfDay(
                result => { finished = result; },
                dayText
            )
        );

        if (!finished)
        {
            pause_manager.SetPause(false);
            yield break;
        }

        patientDayValidator.ResetAllPatientsForNewDay();

        if (blackPanel != null)
        {
            blackPanel.gameObject.SetActive(true);
        }

        yield return Fade(1);

        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        onFinished?.Invoke();
    }

    private IEnumerator Fade(float target)
    {
        if (blackPanel == null) yield break;

        float start = blackPanel.color.a;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            Color color = blackPanel.color;
            color.a = Mathf.Lerp(start, target, t / fadeTime);
            blackPanel.color = color;
            yield return null;
        }

        Color finalColor = blackPanel.color;
        finalColor.a = target;
        blackPanel.color = finalColor;
    }
}
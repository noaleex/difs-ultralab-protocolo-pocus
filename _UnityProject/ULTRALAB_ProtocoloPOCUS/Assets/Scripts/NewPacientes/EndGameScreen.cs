using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class EndGameScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private GameObject finalBom;
    [SerializeField] private GameObject finalRuim;

    [Header("Áudio dos finais")]
    [Tooltip("Evento FMOD tocado uma vez no FinalPositivo. Ex.: event:/Cutscene/videoplayback")]
    [SerializeField] private EventReference finalPositiveSound;

    [Tooltip("Evento FMOD tocado uma vez no FinalNegativo. Atribua fx_triste (2) no Inspector.")]
    [SerializeField] private EventReference finalNegativeSound;

    [Tooltip("Fallback usado se a referência acima ainda não tiver sido atribuída no projeto FMOD.")]
    [SerializeField] private string finalNegativeEventPath = "event:/Cutscene/fx_triste (2)";

    private bool isFinalBom = false;
    private bool endingSoundStarted;
    private EventInstance endingSoundInstance;

    private void Start()
    {
        Time.timeScale = 1f;

        bool positive = GameEndingManager.LastEnding ==
                        GameEndingManager.EndingType.Positive;

        isFinalBom = positive;

        if (titleText != null)
        {
            titleText.text = positive ? "Fim" : "Fim";
        }

        if (messageText != null)
        {
            messageText.text = positive
                ? "Você conseguiu cuidar de todos os pacientes!"
                : "Infelizmente, um paciente não resistiu.";
        }

        if (finalBom != null)
            finalBom.SetActive(positive);

        if (finalRuim != null)
            finalRuim.SetActive(!positive);

        PlayEndingSound(positive);
        UpdateSonarIconState();
    }

    private void Update()
    {
        // O painel ativo é a fonte real do estado visual do final. Se o Sonar
        // sorridente deixar de aparecer, o ícone normal volta a ser mostrado.
        UpdateSonarIconState();
    }

    private void UpdateSonarIconState()
    {
        SonarController sonar = SonarController.Instance;
        if (sonar == null)
            return;

        bool sonarIsSmiling = finalBom != null && finalBom.activeInHierarchy;
        sonar.SetIdleIconVisible(!sonarIsSmiling);
    }

    private void PlayEndingSound(bool positive)
    {
        if (endingSoundStarted)
            return;

        endingSoundStarted = true;

        EventReference configuredSound = positive
            ? finalPositiveSound
            : finalNegativeSound;

        if (!configuredSound.IsNull)
        {
            endingSoundInstance = RuntimeManager.CreateInstance(configuredSound);
        }
        else if (!positive && !string.IsNullOrWhiteSpace(finalNegativeEventPath))
        {
            // Permite testar o caminho do evento enquanto o EventReference ainda
            // não foi sincronizado pelo FMOD Unity Integration.
            try
            {
                endingSoundInstance = RuntimeManager.CreateInstance(finalNegativeEventPath);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning(
                    $"[EndGameScreen] Evento FMOD negativo indisponível: {finalNegativeEventPath}. " +
                    $"Atribua-o em Final Negative Sound. ({exception.Message})",
                    this
                );
                return;
            }
        }
        else
        {
            Debug.LogWarning(
                "[EndGameScreen] Evento FMOD do final não foi atribuído.",
                this
            );
            return;
        }

        if (!endingSoundInstance.isValid())
        {
            Debug.LogError(
                "[EndGameScreen] Não foi possível criar a instância do áudio do final.",
                this
            );
            return;
        }

        FMOD.RESULT result = endingSoundInstance.start();
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError(
                $"[EndGameScreen] Falha ao iniciar o áudio do final: {result}",
                this
            );
            StopEndingSound();
        }
    }

    private void StopEndingSound()
    {
        if (!endingSoundInstance.isValid())
            return;

        endingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        endingSoundInstance.release();
        endingSoundInstance.clearHandle();
    }

    private void OnDestroy()
    {
        StopEndingSound();

        // Se o HUD ainda existir, o ícone nunca fica escondido por causa desta tela.
        if (SonarController.Instance != null)
            SonarController.Instance.RestoreIdleIcon();
    }
}

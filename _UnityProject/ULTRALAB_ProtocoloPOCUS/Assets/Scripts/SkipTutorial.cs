using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipTutorial : MonoBehaviour
{
    [Header("Cena de destino")]
    [SerializeField] private string nextSceneName = "Fase1";

    private bool transitionStarted;

    public void SkipTutorialButton()
    {
        if (transitionStarted)
            return;

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("Cena de destino não configurada.");
            return;
        }

        transitionStarted = true;

        // Prepara o spawn antes de carregar a próxima cena.
        if (boot_manager.instance != null)
        {
            boot_manager.instance.PrepareTutorialExitSpawn(
                nextSceneName);
        }

        PauseController.SetPause(false);

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.RefreshReferences();
            PlayerReferences.Instance.EnablePlayer();
        }

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SkipTutorial : MonoBehaviour
{
    [Header("Cena de destino")]
    [SerializeField] private string nextSceneName = "Fase1";

    private bool transitionStarted;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            SkipTutorialButton();
    }

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

        if (SonarController.Instance != null)
            SonarController.Instance.EncerrarFala();

        if (dialogue_hud.instance != null)
            dialogue_hud.instance.CloseDialogue();

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

        // Remove o próprio controle de skip antes de iniciar o mesmo fluxo de
        // transição usado pela conclusão normal do tutorial.
        gameObject.SetActive(false);

        if (loading_manager.instance == null)
        {
            Debug.LogError("[SkipTutorial] loading_manager não encontrado; transição cancelada.", this);
            return;
        }

        loading_manager.instance.SwitchScene(nextSceneName);
    }
}

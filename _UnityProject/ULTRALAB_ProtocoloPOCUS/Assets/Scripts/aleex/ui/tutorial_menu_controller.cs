using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class tutorial_menu_controller : MonoBehaviour
{
    [FormerlySerializedAs("tutorialProntuarioPanel")]
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private GameObject prontuarioPanel;
    [SerializeField] private GameObject learningPanel;
    [SerializeField] private string mainMenuSceneName = "main_menu";

    private GameObject activeNavigationPanel;

    private void OnEnable()
    {
        // Rouba a prioridade do botão ESC para este script
        pause_manager.overridePauseAction += ToggleMenu;
    }

    private void OnDisable()
    {
        // Devolve a prioridade ao sair da cena ou desativar o painel
        pause_manager.overridePauseAction -= ToggleMenu;
    }

    public void ToggleMenu()
    {
        if (PausePanel != null && PausePanel.activeSelf)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (activeNavigationPanel != null)
            activeNavigationPanel.SetActive(false);

        activeNavigationPanel = null;

        if (PausePanel != null)
            PausePanel.SetActive(true);

        pause_manager.SetPause(true);
    }

    public void CloseMenu()
    {
        if (activeNavigationPanel != null)
            activeNavigationPanel.SetActive(false);

        activeNavigationPanel = null;

        if (PausePanel != null)
            PausePanel.SetActive(false);

        pause_manager.SetPause(false);
    }

    public void OpenProntuario()
    {
        OpenNavigationPanel(prontuarioPanel, nameof(prontuarioPanel));
    }

    public void OpenLearningPanel()
    {
        OpenNavigationPanel(learningPanel, nameof(learningPanel));
    }

    public void BackToPauseMenu()
    {
        OpenMenu();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void ReturnToMenu()
    {
        if (!CanReturnToMenu())
            return;

        TransitionToMenu();
    }

    public void SaveAndReturnToMenu()
    {
        if (save_manager.instance == null || save_manager.instance.currentData == null)
        {
            Debug.LogError("Não foi possível salvar: save_manager ou currentData não está disponível.", this);
            return;
        }

        if (!CanReturnToMenu())
            return;

        save_manager.instance.SaveGame();
        TransitionToMenu();
    }

    private void OpenNavigationPanel(GameObject panel, string panelName)
    {
        if (panel == null)
        {
            Debug.LogError($"O painel {panelName} não foi configurado no Inspector.", this);
            return;
        }

        if (activeNavigationPanel != null && activeNavigationPanel != panel)
            activeNavigationPanel.SetActive(false);

        if (PausePanel != null && PausePanel != panel)
            PausePanel.SetActive(false);

        activeNavigationPanel = panel;
        panel.SetActive(true);
        pause_manager.SetPause(true);
    }

    private bool CanReturnToMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("O nome da cena do menu não foi configurado.", this);
            return false;
        }

        if (loading_manager.instance == null)
        {
            Debug.LogError("Não foi possível trocar de cena: loading_manager não está disponível.", this);
            return false;
        }

        return true;
    }

    private void TransitionToMenu()
    {
        if (activeNavigationPanel != null)
            activeNavigationPanel.SetActive(false);

        activeNavigationPanel = null;

        if (PausePanel != null)
            PausePanel.SetActive(false);

        pause_manager.SetPause(false);
        loading_manager.instance.SwitchScene(mainMenuSceneName);
    }
}
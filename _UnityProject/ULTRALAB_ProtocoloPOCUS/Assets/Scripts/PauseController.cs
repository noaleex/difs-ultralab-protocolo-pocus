using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseController : MonoBehaviour
{
    public static bool IsGamePaused { get; private set; }
    [SerializeField] private ImageAnexx imageAnexx;

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject prontuarioPanel;
    [SerializeField] private GameObject learningPanel;
    [SerializeField] private string mainMenuSceneName = "main_menu";

    private GameObject activeNavigationPanel;
    private int lastPauseToggleFrame = -1;

    private void Awake()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
    }

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
        {
            TogglePause();
        }
    }

    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;

        Time.timeScale = pause ? 0f : 1f;
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;

        TogglePause();
    }

    private void PauseGame()
    {
        if (activeNavigationPanel != null)
            activeNavigationPanel.SetActive(false);

        activeNavigationPanel = null;
        SetPause(true);

        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        /*if (imageAnexx != null)
            imageAnexx.UpdateUI();*/
    }

    private void ResumeGame()
    {
        if (activeNavigationPanel != null)
            activeNavigationPanel.SetActive(false);

        activeNavigationPanel = null;
        SetPause(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void OnMenu()
    {
        TogglePause();
    }

    private void TogglePause()
    {
        if (lastPauseToggleFrame == Time.frameCount)
            return;

        lastPauseToggleFrame = Time.frameCount;

        if (IsGamePaused)
            ResumeGame();

        else
            PauseGame();
    }

    public void ClosePausePanel()
    {
        ResumeGame();
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
        if (activeNavigationPanel != null)
            activeNavigationPanel.SetActive(false);

        activeNavigationPanel = null;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        SetPause(true);
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

        if (pausePanel != null && pausePanel != panel)
            pausePanel.SetActive(false);

        activeNavigationPanel = panel;
        panel.SetActive(true);
        SetPause(true);
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

        if (pausePanel != null)
            pausePanel.SetActive(false);

        SetPause(false);
        loading_manager.instance.SwitchSceneWithoutUpdatingSave(mainMenuSceneName);
    }
}
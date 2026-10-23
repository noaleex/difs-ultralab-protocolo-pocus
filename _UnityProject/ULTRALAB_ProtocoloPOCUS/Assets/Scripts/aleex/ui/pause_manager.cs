using UnityEngine;
using UnityEngine.InputSystem;

public class pause_manager : MonoBehaviour
{
    public static pause_manager instance { get; private set; }

    public static bool isGamePaused { get; private set; }

    [Header("UI do Pause")]
    [SerializeField] private GameObject pausePanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Time.timeScale = 1f;
        isGamePaused = false;
    }

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    // Mantido estático para facilitar o uso por outros scripts (ex: OpenExams, DayTransition)
    public static void SetPause(bool pause)
    {
        isGamePaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    public void TogglePause()
    {
        if (isGamePaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        SetPause(true);
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        SetPause(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    // Para o botão da interface chamar diretamente
    public void OnMenu()
    {
        TogglePause();
    }

    // Para uso com o novo Input System (tecla ESC ou Start no controle)
    public void OnPauseInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            TogglePause();
        }
    }
}

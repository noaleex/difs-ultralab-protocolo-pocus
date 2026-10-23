using UnityEngine;
using UnityEngine.InputSystem;

public class pause_manager : MonoBehaviour
{
    public static pause_manager instance { get; private set; }
    public static bool isGamePaused { get; private set; }
    public static System.Action overridePauseAction;

    [Header("UI do Pause")]
    [SerializeField] private GameObject pausePanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.SetParent(null); // Garante que não tenha pai, prevenindo warnings
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

    private void Update()
    {
        // Leitura direta do hardware: ignora configurações quebradas do PlayerInput
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TriggerPauseAction();
        }
    }

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

    public void OnMenu()
    {
        TogglePause();
    }

    // Mantido caso você ainda queira usar botões de controle (Gamepad) via PlayerInput
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            TriggerPauseAction();
        }
    }

    private void TriggerPauseAction()
    {
        if (overridePauseAction != null)
        {
            overridePauseAction.Invoke();
        }
        else
        {
            TogglePause();
        }
    }
}
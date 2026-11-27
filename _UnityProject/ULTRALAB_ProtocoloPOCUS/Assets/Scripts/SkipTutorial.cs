using UnityEngine;
using UnityEngine.InputSystem;

public static class SkipKeyInput
{
    private static bool wasDown;

    public static bool WasPressedThisFrame()
    {
        if (Keyboard.current == null)
        {
            wasDown = false;
            return false;
        }

        bool isDown = Keyboard.current.pKey.isPressed;
        bool pressed = Keyboard.current.pKey.wasPressedThisFrame || (isDown && !wasDown);
        wasDown = isDown;
        return pressed;
    }
}

public static class SkipTouchInput
{
    private static bool waitingForSecondTap;
    private static bool wasPressed;
    private static float firstTapTime = float.NegativeInfinity;
    private static Vector2 firstTapPosition;

    public static bool WasDoubleTapThisFrame(float maxInterval, float maxDistance)
    {
        Touchscreen touchscreen = Touchscreen.current;
        bool isPressed = touchscreen != null && touchscreen.primaryTouch.press.isPressed;
        bool pressedThisFrame = touchscreen != null &&
                                (touchscreen.primaryTouch.press.wasPressedThisFrame ||
                                 (isPressed && !wasPressed));
        wasPressed = isPressed;

        if (!pressedThisFrame)
            return false;

        float now = Time.unscaledTime;
        Vector2 position = touchscreen.primaryTouch.position.ReadValue();
        bool isSecondTap = waitingForSecondTap &&
                           now - firstTapTime <= maxInterval &&
                           Vector2.Distance(position, firstTapPosition) <= maxDistance;

        if (isSecondTap)
        {
            Reset();
            return true;
        }

        waitingForSecondTap = true;
        firstTapTime = now;
        firstTapPosition = position;
        return false;
    }

    public static void Reset()
    {
        waitingForSecondTap = false;
        wasPressed = false;
        firstTapTime = float.NegativeInfinity;
        firstTapPosition = default;
    }
}

public class SkipTutorial : MonoBehaviour
{
    public static bool IsSkipKeyReserved { get; private set; }

    [Header("Cena de destino")]
    [SerializeField] private string nextSceneName = "Fase1";

    [Header("Gesto Android")]
    [Tooltip("Tempo mÃ¡ximo entre os dois toques para considerar toque duplo.")]
    [SerializeField, Min(0.05f)] private float doubleTapInterval = 0.3f;
    [Tooltip("DistÃ¢ncia mÃ¡xima, em pixels, entre os dois toques.")]
    [SerializeField, Min(0f)] private float doubleTapMaxDistance = 120f;

    private bool transitionStarted;

    private void OnEnable()
    {
        IsSkipKeyReserved = true;
        SkipTouchInput.Reset();
    }

    private void OnDisable()
    {
        IsSkipKeyReserved = false;
        SkipTouchInput.Reset();
    }

    private void Update()
    {
        bool skipByKey = SkipKeyInput.WasPressedThisFrame();
        bool skipByDoubleTap = SkipTouchInput.WasDoubleTapThisFrame(
            doubleTapInterval,
            doubleTapMaxDistance);

        if (skipByKey || skipByDoubleTap)
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

        // Prepara o spawn antes de carregar a próxima cena.
        PauseController.SetPause(false);
        pause_manager.SetPause(false);

        if (SonarController.Instance != null)
            SonarController.Instance.CancelDialogueForSceneTransition();

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.RefreshReferences();
            PlayerReferences.Instance.DisablePlayer();
        }

        if (loading_manager.instance == null)
        {
            transitionStarted = false;
            Debug.LogError("loading_manager não está disponível para pular o tutorial.", this);
            return;
        }

        // O boot_manager reativa o mesmo jogador e inicializa a UTI no destino.
        loading_manager.instance.SwitchScene(nextSceneName);
    }
}

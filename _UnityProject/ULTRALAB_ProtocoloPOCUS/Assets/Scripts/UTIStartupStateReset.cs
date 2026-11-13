using UnityEngine;

[DisallowMultipleComponent]
public sealed class UTIStartupStateReset : MonoBehaviour
{
    private void Awake()
    {
        // The pause_manager persists from _boot and can carry a stale pause
        // from a previous menu/dialogue. UTI starts as active gameplay.
        PauseController.SetPause(false);

        if (pause_manager.instance != null)
            pause_manager.SetPause(false);
        else
            Time.timeScale = 1f;
    }
}

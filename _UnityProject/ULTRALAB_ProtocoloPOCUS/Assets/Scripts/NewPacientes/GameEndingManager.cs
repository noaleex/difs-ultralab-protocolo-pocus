using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Controla a transição para a cena final e impede chamadas duplicadas.</summary>
public static class GameEndingManager
{
    public enum EndingType { Positive, Negative }

    public const string EndGameSceneName = "EndGame";
    public static EndingType LastEnding { get; private set; } = EndingType.Negative;
    public static bool IsEnding { get; private set; }

    public static void FinishGame(EndingType ending)
    {
        if (IsEnding) return;
        IsEnding = true;
        LastEnding = ending;
        if (pause_manager.instance != null)
            pause_manager.SetPause(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(EndGameSceneName);
    }

    public static void ResetEndingState()
    {
        IsEnding = false;
        LastEnding = EndingType.Negative;
    }
}

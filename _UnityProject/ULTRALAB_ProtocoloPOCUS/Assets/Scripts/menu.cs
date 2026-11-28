using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "main_menu";

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        GameEndingManager.ResetEndingState();

        // Garante que nenhum dos dois sistemas de pause fique preso
        pause_manager.SetPause(false);
        PauseController.SetPause(false);

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchSceneWithoutUpdatingSave(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{
    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("main_menu");
    }
}

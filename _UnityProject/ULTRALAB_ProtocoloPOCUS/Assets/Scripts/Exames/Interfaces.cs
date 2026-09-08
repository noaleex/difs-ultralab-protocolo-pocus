using UnityEngine;
using UnityEngine.SceneManagement;

public class Interfaces : MonoBehaviour
{
    [SerializeField] private GameObject pocus;
    [SerializeField] private GameObject leito;
    [SerializeField] private GameObject tool;

    [SerializeField] private string lab;
    [SerializeField] private string uti;


    // =====================================================
    // POCUS
    // =====================================================

    public void PocusClick()
    {
        pocus.SetActive(true);

        AudioManager.Instance?.PlayInteraction();
    }


    // =====================================================
    // LEITO
    // =====================================================

    public void LeitoClick()
    {
        leito.SetActive(true);

        AudioManager.Instance?.PlayInteraction();
    }


    // =====================================================
    // TOOL
    // =====================================================

    public void ToolClick()
    {
        tool.SetActive(true);

        AudioManager.Instance?.PlayInteraction();
    }


    // =====================================================
    // VOLTAR INTERFACE
    // =====================================================

    public void BackInterface()
    {
        pocus.SetActive(false);
        leito.SetActive(false);
        tool.SetActive(false);

        AudioManager.Instance?.PlayBack();
    }


    // =====================================================
    // VOLTAR PARA CENA DO PACIENTE
    // =====================================================

    public void BackExam()
    {
        AudioManager.Instance?.PlayBack();


        // =================================================
        // SALVAR TEMPO ATUAL
        // =================================================

        Timer timer =
            FindFirstObjectByType<Timer>();


        if (timer != null)
        {
            timer.SaveCurrentTime();
        }
        else
        {
            Debug.LogWarning(
                "Timer não encontrado na cena de exames."
            );
        }


        // =================================================
        // VERIFICAR PACIENTE ATUAL
        // =================================================

        if (CurrentPatient.Data == null)
        {
            Debug.LogError(
                "CurrentPatient.Data está vazio ao voltar dos exames."
            );

            return;
        }


        // =================================================
        // ESCOLHER CENA
        // =================================================

        string sceneToLoad;


        if (CurrentPatient.Data.tutorial)
        {
            sceneToLoad = lab;
        }
        else
        {
            sceneToLoad = uti;
        }


        // =================================================
        // PREPARAR PLAYER
        // =================================================

        PauseController.SetPause(false);


        SceneManager.sceneLoaded +=
            OnSceneLoaded;


        // =================================================
        // CARREGAR CENA
        // =================================================

        SceneManager.LoadScene(
            sceneToLoad
        );
    }


    // =====================================================
    // CENA CARREGADA
    // =====================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;


        // =================================================
        // PLAYER
        // =================================================

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance
                .RefreshReferences();


            PlayerReferences.Instance
                .EnablePlayer();


            if (PlayerReferences.Instance
                .InteractIcon != null)
            {
                PlayerReferences.Instance
                    .InteractIcon
                    .SetActive(true);
            }
        }
    }
}
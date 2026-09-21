using UnityEngine;
using UnityEngine.SceneManagement;

public class Interfaces : MonoBehaviour
{
    [Header("Interfaces")]
    [SerializeField] private GameObject pocus;
    [SerializeField] private GameObject leito;
    [SerializeField] private GameObject tool;


    [Header("Cenas")]
    [SerializeField] private string lab;
    [SerializeField] private string uti;


    private void Start()
    {
        BackInterface();
    }


    // =====================================================
    // POCUS
    // =====================================================

    public void PocusClick()
    {
        if (pocus != null)
        {
            pocus.SetActive(true);
        }


        AudioManager.Instance?.PlayInteraction();
    }


    // =====================================================
    // LEITO
    // =====================================================

    public void LeitoClick()
    {
        if (leito != null)
        {
            leito.SetActive(true);
        }


        AudioManager.Instance?.PlayInteraction();
    }


    // =====================================================
    // TOOL
    // =====================================================

    public void ToolClick()
    {
        if (tool != null)
        {
            tool.SetActive(true);
        }


        AudioManager.Instance?.PlayInteraction();
    }


    // =====================================================
    // VOLTAR INTERFACE
    // =====================================================

    public void BackInterface()
    {
        if (pocus != null)
        {
            pocus.SetActive(false);
        }


        if (leito != null)
        {
            leito.SetActive(false);
        }


        if (tool != null)
        {
            tool.SetActive(false);
        }


        AudioManager.Instance?.PlayBack();
    }


    // =====================================================
    // VOLTAR PARA CENA DO PACIENTE
    // =====================================================

    public void BackExam()
    {
        AudioManager.Instance?.PlayBack();


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


        // =================================================
        // CARREGAR CENA
        // =================================================

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(
                sceneToLoad
            );
        }
        else
        {
            SceneManager.LoadScene(
                sceneToLoad
            );
        }
    }
}
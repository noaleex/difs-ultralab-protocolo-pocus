using UnityEngine;
using UnityEngine.SceneManagement;

public class Interfaces : MonoBehaviour
{
    [Header("Interfaces")]
    [SerializeField] private GameObject pocus;
    [SerializeField] private GameObject leito;
    [SerializeField] private GameObject tool;
    [SerializeField] private GameObject infos;
    [SerializeField] private GameObject sonar;


    [Header("Cenas")]
    [SerializeField] private string lab;
    [SerializeField] private string uti;


    private void Start()
    {
        BackInterface();
    }


    public void PocusClick()
    {
        if (pocus != null)
        {
            pocus.SetActive(true);
        }


        AudioManager.Instance?.PlayInteraction();
    }

    public void LeitoClick()
    {
        if (leito != null)
        {
            leito.SetActive(true);
        }


        AudioManager.Instance?.PlayInteraction();
    }

    public void SonarClick()
    {
        if (sonar != null)
        {
            sonar.SetActive(true);
        }

        AudioManager.Instance?.PlayInteraction();
    }
    
public void InfosClick()
{
    if (infos != null)
    {
        infos.SetActive(true);

        MonitorMultiparametroUI monitor =
            infos.GetComponentInChildren<MonitorMultiparametroUI>(true);

        if (monitor != null)
        {
            monitor.Refresh();
        }
        else
        {
            Debug.LogWarning(
                "MonitorMultiparametroUI não foi encontrado no painel Infos.",
                infos
            );
        }
    }

    AudioManager.Instance?.PlayInteraction();
}


    public void ToolClick()
    {


        /*if (tool != null)
        {
            tool.SetActive(true);
        }


        AudioManager.Instance?.PlayInteraction();*/
    }

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

        if (infos != null)
        {
            infos.SetActive(false);
        }

        if (sonar != null)
        {
            sonar.SetActive(false);
        }

    }


    public void BackExam()
    {
        AudioManager.Instance?.PlayBack();

        // VERIFICAR PACIENTE ATUAL

        if (CurrentPatient.Data == null)
        {
            Debug.LogError(
                "CurrentPatient.Data está vazio ao voltar dos exames."
            );

            return;
        }

        // ESCOLHER CENA

        string sceneToLoad;


        if (CurrentPatient.Data.tutorial)
        {
            sceneToLoad = lab;
        }
        else
        {
            sceneToLoad = uti;
        }


        PauseController.SetPause(false);


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
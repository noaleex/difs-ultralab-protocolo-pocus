using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

    [Header("Falas do Sonar (Tutorial)")]
    [Tooltip("Fala que roda sozinha ao entrar na cena de Exames.")]
    public NPCdialogue falaIntro;
    [Tooltip("Fala ao clicar no paciente/leito no tutorial.")]
    public NPCdialogue falaLeito;
    [Tooltip("Fala ao clicar no monitor/ventilador no tutorial.")]
    public NPCdialogue falaMonitor;
    [Tooltip("Fala do primeiro clique no ultrassom (Pocus).")]
    public NPCdialogue falaPocusIntro;

    private bool jaFalouPocus = false;

    // Propriedade rápida para saber se estamos com o paciente do tutorial
    private bool IsTutorial => CurrentPatient.Data != null && CurrentPatient.Data.tutorial;

    private void Start()
    {
        BackInterface();

        // Se for o paciente do tutorial, inicia a fala de introdução com um pequeno atraso
        if (IsTutorial)
        {
            StartCoroutine(StartTutorialIntro());
        }
    }

    private IEnumerator StartTutorialIntro()
    {
        // Espera a tela clarear um pouquinho
        yield return new WaitForSeconds(1.0f);

        // Só fala se ainda não tiver falado nesta jogatina
        if (PlayerPrefs.GetInt("Exames_Intro_Tutorial", 0) == 0)
        {
            if (SonarController.Instance != null && falaIntro != null)
            {
                SonarController.Instance.Falar(falaIntro);
                
                // Grava na memória para não repetir se o jogador fechar o exame e voltar
                PlayerPrefs.SetInt("Exames_Intro_Tutorial", 1);
                PlayerPrefs.Save();
            }
        }
    }

    public void PocusClick()
    {
        AudioManager.Instance?.PlayInteraction();

        // No tutorial, o primeiro clique apenas chama o Sonar
        if (IsTutorial && !jaFalouPocus)
        {
            if (SonarController.Instance != null && falaPocusIntro != null)
            {
                SonarController.Instance.Falar(falaPocusIntro);
                jaFalouPocus = true; // Libera o acesso para o próximo clique
            }
            return;
        }

        // Se não for tutorial, ou se já for o segundo clique, abre a interface normal
        if (pocus != null)
        {
            pocus.SetActive(true);
        }
    }

    public void LeitoClick()
    {
        AudioManager.Instance?.PlayInteraction();

        // No tutorial, o Leito apenas chama o Sonar
        if (IsTutorial)
        {
            if (SonarController.Instance != null && falaLeito != null)
            {
                SonarController.Instance.Falar(falaLeito);
            }
            return; // Interrompe aqui para não abrir a interface visual
        }

        // Fluxo normal
        if (leito != null)
        {
            leito.SetActive(true);
        }
    }

    public void InfosClick()
    {
        AudioManager.Instance?.PlayInteraction();

        // No tutorial, o Monitor apenas chama o Sonar
        if (IsTutorial)
        {
            if (SonarController.Instance != null && falaMonitor != null)
            {
                SonarController.Instance.Falar(falaMonitor);
            }
            return; // Interrompe aqui para não abrir a interface visual
        }

        // Fluxo normal
        if (infos != null)
        {
            infos.SetActive(true);

            MonitorMultiparametroUI monitor = infos.GetComponentInChildren<MonitorMultiparametroUI>(true);
            if (monitor != null)
            {
                monitor.Refresh();
            }
            else
            {
                Debug.LogWarning("MonitorMultiparametroUI não foi encontrado no painel Infos.", infos);
            }
        }
    }

    public void SonarClick()
    {
        AudioManager.Instance?.PlayInteraction();

        if (sonar != null)
        {
            sonar.SetActive(true);
        }
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
        if (pocus != null) pocus.SetActive(false);
        if (leito != null) leito.SetActive(false);
        if (tool != null) tool.SetActive(false);
        if (infos != null) infos.SetActive(false);
        if (sonar != null) sonar.SetActive(false);
    }

    public void BackExam()
    {
        AudioManager.Instance?.PlayBack();

        if (CurrentPatient.Data == null)
        {
            Debug.LogError("CurrentPatient.Data está vazio ao voltar dos exames.");
            return;
        }

        string sceneToLoad = CurrentPatient.Data.tutorial ? lab : uti;

        PauseController.SetPause(false);

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.EnablePlayer();

            if (PlayerReferences.Instance.InteractIcon != null)
            {
                PlayerReferences.Instance.InteractIcon.SetActive(true);
            }
        }
    }
}
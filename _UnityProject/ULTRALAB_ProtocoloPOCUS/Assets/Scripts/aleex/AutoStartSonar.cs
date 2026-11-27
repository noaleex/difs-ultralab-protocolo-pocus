using UnityEngine;
using System.Collections;

public class AutoStartSonar : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Arraste o ScriptableObject com a fala de boas-vindas do Sonar.")]
    public NPCdialogue falaDeBoasVindas;

    [Tooltip("Aviso visual que mostra as teclas de movimentação durante a primeira fala.")]
    public GameObject tutorialTeclas;
    
    [Tooltip("Tempo para esperar o fade da tela terminar antes de falar.")]
    public float tempoDeEspera = 1.5f;

    [Tooltip("ID único para não repetir caso o jogador volte a essa cena depois.")]
    public string uniqueId = "Tutorial_Intro_Sonar";

    private void Start()
    {
        // Verifica se essa fala já foi dita. Se sim, não faz nada.
        if (PlayerPrefs.GetInt(uniqueId, 0) == 1) return;

        StartCoroutine(ChamarSonarComAtraso());
    }

    private IEnumerator ChamarSonarComAtraso()
    {
        // Espera o tempo do fade da sua tela de loading
        yield return new WaitForSecondsRealtime(tempoDeEspera);

        if (SonarController.Instance != null && falaDeBoasVindas != null && !SonarController.Instance.isDialogueActive)
        {
            SonarController.Instance.Falar(falaDeBoasVindas);

            // Falar pode recusar a chamada caso outro diálogo esteja ativo.
            if (!SonarController.Instance.isDialogueActive)
                yield break;

            if (tutorialTeclas != null)
                tutorialTeclas.SetActive(true);

            // Grava que ele já falou para não repetir
            PlayerPrefs.SetInt(uniqueId, 1);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("[SONAR] SonarController ou Arquivo de Diálogo não encontrados.");
        }
    }
}

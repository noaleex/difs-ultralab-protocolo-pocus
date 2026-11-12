using UnityEngine;
using TMPro;

public class CharacterSelection : MonoBehaviour
{
    [Header("Configuração de UI")]
    [Tooltip("Arraste aqui o TMP_InputField onde o jogador digita o nome.")]
    public TMP_InputField inputNomeJogador;

    private string cenaParaCarregar = "";

    public void SelecionarPersonagemFeminino()
    {
        PlayerPrefs.SetString("PlayerGender", "Female");
        cenaParaCarregar = "PF0_Intro"; 
        Debug.Log("[SISTEMA] Personagem Feminina selecionada.");
    }

    public void SelecionarPersonagemMasculino()
    {
        PlayerPrefs.SetString("PlayerGender", "Male");
        cenaParaCarregar = "PM0_Intro"; 
        Debug.Log("[SISTEMA] Personagem Masculino selecionado.");
    }

    public void ConfirmarEIniciar()
    {
        if (string.IsNullOrEmpty(cenaParaCarregar))
        {
            Debug.LogWarning("[SISTEMA] O jogador tentou iniciar sem escolher um personagem!");
            return; 
        }

        // 1. Limpa o histórico de falas do tutorial para o Sonar voltar a falar no Novo Jogo!
        LimparDadosDoTutorial();

        // 2. Salva o nome
        if (inputNomeJogador != null && !string.IsNullOrWhiteSpace(inputNomeJogador.text))
        {
            PlayerPrefs.SetString("PlayerName", inputNomeJogador.text.Trim());
        }
        else
        {
            PlayerPrefs.SetString("PlayerName", "Enfermeiro(a)");
        }
        
        PlayerPrefs.Save();

        // 3. Carrega a cena
        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(cenaParaCarregar);
        }
    }

    // Método que formata (apaga) as chaves de memória do Sonar
    private void LimparDadosDoTutorial()
    {
        // Apaga a chave da primeira cena do tutorial
        PlayerPrefs.DeleteKey("Tutorial_Intro_Sonar");
        
        // Apaga a chave da cena de exames
        PlayerPrefs.DeleteKey("Exames_Intro_Tutorial");
        
        // Se você tiver criado outras chaves com nomes diferentes nos seus testes, 
        // basta adicionar uma linha igual a essa abaixo com o nome da chave:
        // PlayerPrefs.DeleteKey("Nome_Da_Sua_Chave_Aqui");

        Debug.Log("[SISTEMA] Memória do tutorial limpa. O Sonar falará normalmente.");
    }
}
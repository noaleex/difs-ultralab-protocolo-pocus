using UnityEngine;
using TMPro; // Necessário para acessar os campos de texto do TextMeshPro

public class CharacterSelection : MonoBehaviour
{
    [Header("Configuração de UI")]
    [Tooltip("Arraste aqui o TMP_InputField onde o jogador digita o nome.")]
    public TMP_InputField inputNomeJogador;

    private string cenaParaCarregar = "";

    // Botão Feminino
    public void SelecionarPersonagemFeminino()
    {
        PlayerPrefs.SetString("PlayerGender", "Female");
        cenaParaCarregar = "PF0_Intro"; 
        Debug.Log("[SISTEMA] Personagem Feminina selecionada. Aguardando Iniciar.");
    }

    // Botão Masculino
    public void SelecionarPersonagemMasculino()
    {
        PlayerPrefs.SetString("PlayerGender", "Male");
        cenaParaCarregar = "PM0_Intro"; 
        Debug.Log("[SISTEMA] Personagem Masculino selecionado. Aguardando Iniciar.");
    }

    // Botão "Começar" / "Jogar"
    public void ConfirmarEIniciar()
    {
        if (string.IsNullOrEmpty(cenaParaCarregar))
        {
            Debug.LogWarning("[SISTEMA] O jogador tentou iniciar sem escolher um personagem!");
            return; 
        }

        // Verifica se o campo de input existe e se o jogador digitou algo
        if (inputNomeJogador != null && !string.IsNullOrWhiteSpace(inputNomeJogador.text))
        {
            // Salva o nome digitado
            PlayerPrefs.SetString("PlayerName", inputNomeJogador.text.Trim());
        }
        else
        {
            // Nome padrão de segurança caso o jogador deixe o campo em branco
            PlayerPrefs.SetString("PlayerName", "Enfermeiro(a)");
        }
        
        PlayerPrefs.Save(); // Salva tanto o nome quanto o gênero definitivamente

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(cenaParaCarregar);
        }
    }
}
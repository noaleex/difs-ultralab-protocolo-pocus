using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    private string cenaParaCarregar = "";

    // Botão Feminino
    public void SelecionarPersonagemFeminino()
    {
        PlayerPrefs.SetString("PlayerGender", "Female");
        PlayerPrefs.Save();
        
        cenaParaCarregar = "PF0_Intro"; // Apenas guarda o nome da cena
        Debug.Log("[SISTEMA] Personagem Feminina selecionada. Aguardando Iniciar.");
    }

    // Botão Masculino
    public void SelecionarPersonagemMasculino()
    {
        PlayerPrefs.SetString("PlayerGender", "Male");
        PlayerPrefs.Save();
        
        cenaParaCarregar = "PM0_Intro"; // Apenas guarda o nome da cena
        Debug.Log("[SISTEMA] Personagem Masculino selecionado. Aguardando Iniciar.");
    }

    // Botão "Começar" / "Jogar"
    public void ConfirmarEIniciar()
    {
        if (string.IsNullOrEmpty(cenaParaCarregar))
        {
            Debug.LogWarning("[SISTEMA] O jogador tentou iniciar sem escolher um personagem!");
            return; // Impede de iniciar se não escolheu ninguém
        }

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(cenaParaCarregar);
        }
    }
}
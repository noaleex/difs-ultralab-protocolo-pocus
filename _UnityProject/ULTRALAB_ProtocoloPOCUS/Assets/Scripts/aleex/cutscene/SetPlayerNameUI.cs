using UnityEngine;
using TMPro;

public class SetPlayerNameUI : MonoBehaviour
{
    [Tooltip("O componente de texto que receberá o nome do jogador.")]
    public TMP_Text nameText;

    [Tooltip("Chave usada no PlayerPrefs para salvar o nome do jogador na tela de criação.")]
    public string playerPrefsKey = "PlayerName"; 

    [Tooltip("Nome que aparecerá caso não encontre nenhum save (ex: rodando a cena solta para teste).")]
    public string defaultName = "Estudante de Enfermagem";

    private void Start()
    {
        // Se não tiver arrastado o texto no Inspector, ele tenta pegar o componente automaticamente
        if (nameText == null)
            nameText = GetComponent<TMP_Text>();

        if (nameText != null)
        {
            // Puxa o nome salvo e aplica no certificado
            nameText.text = PlayerPrefs.GetString(playerPrefsKey, defaultName);
        }
    }
}
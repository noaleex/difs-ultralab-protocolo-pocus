using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialPrompt : MonoBehaviour
{
    public enum TipoTutorial { Interagir, Movimento }
    
    [Header("Configuração")]
    [Tooltip("Escolha o que o jogador precisa apertar para este aviso sumir.")]
    public TipoTutorial acaoParaSumir = TipoTutorial.Interagir;

    private void Update()
    {
        if (VerificarInput())
        {
            // Desativa o objeto inteiro (assim não pesa na cena e some da tela)
            gameObject.SetActive(false); 
            
            // Se preferir DELETAR de vez da memória, use a linha abaixo em vez da de cima:
            // Destroy(gameObject);
        }
    }

    private bool VerificarInput()
    {
        if (acaoParaSumir == TipoTutorial.Interagir)
        {
            // Some se apertar Botão Esquerdo, Espaço ou E
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)) return true;
        }
        else if (acaoParaSumir == TipoTutorial.Movimento)
        {
            // Some se apertar W, A, S ou D
            if (Keyboard.current != null && (
                Keyboard.current.wKey.wasPressedThisFrame || 
                Keyboard.current.aKey.wasPressedThisFrame || 
                Keyboard.current.sKey.wasPressedThisFrame || 
                Keyboard.current.dKey.wasPressedThisFrame))
            {
                return true;
            }
        }

        return false;
    }
}
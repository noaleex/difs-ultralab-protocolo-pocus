using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class dialogue_hud : MonoBehaviour
{
    public static dialogue_hud instance { get; private set; }

    [Header("Referências da Interface")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CloseDialogue();
    }

    public void CloseDialogue()
    {
        if (dialogueText != null)
            dialogueText.SetText(string.Empty);
        if (nameText != null)
            nameText.SetText(string.Empty);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}

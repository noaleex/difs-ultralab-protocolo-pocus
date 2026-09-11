using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    public bool isNarrator; 
    public string characterName;
    [TextArea(2, 5)]
    public string sentence;
}

[System.Serializable]
public class CutsceneSequence
{
    public string cutsceneID;
    public DialogueLine[] lines;
}

public class text_manager : MonoBehaviour
{
    public static text_manager Instance {get; private set;}

    [Header("Configurações de UI")]
    [SerializeField] private GameObject dialogueMasterContainer;
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Banco de Diálogos")]
    [SerializeField] private CutsceneSequence[] allCutscenes;

    private CutsceneSequence currentSequence;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dialogueMasterContainer != null) dialogueMasterContainer.SetActive(false);
    }

    public void StartCutsceneDialogue (string id)
    {
        currentSequence = System.Array.Find(allCutscenes, c => c.cutsceneID == id);
        if (currentSequence == null) return;

        currentLineIndex = 0;
        dialogueMasterContainer.SetActive(true);
        PlayNextLine();
    }

    public void PlayNextLine ()
    {
        if (currentSequence == null) return;

        if (currentLineIndex < currentSequence.lines.Length)
        {
            DialogueLine nextLine = currentSequence.lines[currentLineIndex];

            if (nextLine.isNarrator)
            {
                if (backgroundPanel != null) backgroundPanel.SetActive(false);
                if (nameText != null) nameText.gameObject.SetActive(false);
            }
            else
            {
                if (backgroundPanel != null) backgroundPanel.SetActive(true);
                if (nameText != null)
                {
                    nameText.gameObject.SetActive(true);
                    nameText.text = nextLine.characterName;
                }
            }

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypewriterEffect(nextLine.sentence));
            
            currentLineIndex++;
        } 
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueMasterContainer.SetActive(false);
        currentSequence = null;
    }

    private IEnumerator TypewriterEffect(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class character_portrait_entry
{
    public string characterName;
    public Sprite portraitSprite;
}

[Serializable]
public class dialogue_line
{
    public bool isNarrator;
    public string characterName;
    [TextArea(2, 5)]
    public string sentence;
}

[Serializable]
public class cutscene_sequence
{
    public string cutsceneId;
    public dialogue_line[] lines;
}

public class text_cutscene_manager : MonoBehaviour
{
    public static text_cutscene_manager instance { get; private set; }

    [Header("Configurações de UI")]
    [SerializeField] private GameObject dialogueMasterContainer;

    [SerializeField] private GameObject narratorContainer;
    [SerializeField] private TextMeshProUGUI narratorText;

    [SerializeField] private GameObject characterDialogueContainer;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterDialogueText;
    [SerializeField] private Image characterPortraitImage;

    [SerializeField] private List<character_portrait_entry> characterPortraits = new List<character_portrait_entry>();

    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Banco de Diálogos")]
    [SerializeField] private cutscene_sequence[] allCutscenes;

    private cutscene_sequence currentSequence;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (dialogueMasterContainer != null)
        {
            dialogueMasterContainer.SetActive(false);
        }
    }

    public void StartCutsceneDialogue(string id)
    {
        currentSequence = Array.Find(allCutscenes, c => c.cutsceneId == id);
        if (currentSequence == null) return;

        currentLineIndex = 0;
        if (dialogueMasterContainer != null) dialogueMasterContainer.SetActive(true);
        PlayNextLine();
    }

    public void PlayNextLine()
    {
        if (currentSequence == null) return;

        if (currentLineIndex < currentSequence.lines.Length)
        {
            dialogue_line nextLine = currentSequence.lines[currentLineIndex];

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            if (nextLine.isNarrator)
            {
                if (characterDialogueContainer != null) characterDialogueContainer.SetActive(false);
                if (narratorContainer != null) narratorContainer.SetActive(true);

                typingCoroutine = StartCoroutine(TypewriterEffect(narratorText, nextLine.sentence));
            }
            else
            {
                if (narratorContainer != null) narratorContainer.SetActive(false);
                if (characterDialogueContainer != null) characterDialogueContainer.SetActive(true);

                if (characterNameText != null)
                {
                    characterNameText.text = nextLine.characterName;
                }

                UpdatePortrait(nextLine.characterName);

                typingCoroutine = StartCoroutine(TypewriterEffect(characterDialogueText, nextLine.sentence));
            }

            currentLineIndex++;
        }
        else
        {
            EndDialogue();
        }
    }

    private void UpdatePortrait(string characterName)
    {
        if (characterPortraitImage == null) return;

        character_portrait_entry entry = characterPortraits.Find(p => p.characterName.Equals(characterName, StringComparison.OrdinalIgnoreCase));

        if (entry != null && entry.portraitSprite != null)
        {
            characterPortraitImage.gameObject.SetActive(true);
            characterPortraitImage.sprite = entry.portraitSprite;
        }
        else
        {
            characterPortraitImage.gameObject.SetActive(false);
        }
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (narratorContainer != null) narratorContainer.SetActive(false);
        if (characterDialogueContainer != null) characterDialogueContainer.SetActive(false);
        if (dialogueMasterContainer != null) dialogueMasterContainer.SetActive(false);

        currentSequence = null;
    }

    private IEnumerator TypewriterEffect(TextMeshProUGUI targetText, string sentence)
    {
        if (targetText == null) yield break;

        targetText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            targetText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.InputSystem;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCdialogue dialogueData;
    public EventReference interactSoundEvent;

    [Header("Configuração")]
    public string uniqueId;
    public bool startDialogueOnStart;
    public bool isInterfaceCharacter;
    public bool startDialogueOnTrigger;
    public bool triggerOnlyOnce = true;

    private EventInstance dialogueVoiceInstance;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool hasTriggeredDialogue;
    private bool dialogoJaConcluido;

    public bool IsDialogueActive => isDialogueActive;
    public System.Action OnDialogueEnded;

    private static System.Collections.Generic.HashSet<string> completedDialogues = new System.Collections.Generic.HashSet<string>();

    void Start()
    {
        if (startDialogueOnStart)
        {
            // Verifica se já falou isso antes em algum momento do jogo (Sistema de Memória)
            if (triggerOnlyOnce && !string.IsNullOrEmpty(uniqueId) && completedDialogues.Contains(uniqueId))
            {
                return; 
            }

            StartDialogueExternally();
        }
    }

    void Update()
    {
        if (isInterfaceCharacter && isDialogueActive)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                NextLine();
            }
        }
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null || (pause_manager.isGamePaused && !isDialogueActive))
            return;

        // Não permite reiniciar o diálogo já concluído
        if (dialogoJaConcluido && !isInterfaceCharacter)
            return;

        PlayInteractSound();

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void PlayInteractSound()
    {
        if (!interactSoundEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(interactSoundEvent);
        }
    }

    public void StartDialogueExternally()
    {
        if (dialogueData == null || isDialogueActive)
            return;

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        if (!isInterfaceCharacter)
        {
            if (PlayerReferences.Instance?.InteractionDetector != null)
            {
                PlayerReferences.Instance.InteractionDetector.ForceInteractable(this);
            }
        }

        PlayInteractSound();
        StartDialogue();
    }

    public void StartDialogue(NPCdialogue newDialogue)
    {
        if (newDialogue == null || isDialogueActive)
            return;

        dialogueData = newDialogue;
        StartDialogueExternally();
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        dialogue_hud.instance.nameText.SetText(dialogueData.npcName);
        dialogue_hud.instance.portraitImage.sprite = dialogueData.npcPortrait;
        dialogue_hud.instance.dialoguePanel.SetActive(true);

        pause_manager.SetPause(true);

        StartCoroutine(TypeLine());

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.InteractIcon.SetActive(false);

            if (PlayerReferences.Instance.playerMovement != null)
            {
                PlayerReferences.Instance.DisablePlayer();
            }
        }
    }

    public void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            StopVoice();

            dialogue_hud.instance.dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogue_hud.instance.dialogueText.SetText("");

        StartVoice();

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogue_hud.instance.dialogueText.text += letter;

            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        StopVoice();

        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void StartVoice()
    {
        if (dialogueData.voiceEvent.IsNull)
            return;

        StopVoice();

        dialogueVoiceInstance = RuntimeManager.CreateInstance(dialogueData.voiceEvent);
        dialogueVoiceInstance.start();
    }

    void StopVoice()
    {
        if (dialogueVoiceInstance.isValid())
        {
            dialogueVoiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            dialogueVoiceInstance.release();
            dialogueVoiceInstance = default;
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        StopVoice();

        isDialogueActive = false;

        if (!isInterfaceCharacter)
        {
            dialogoJaConcluido = true;
        }

        // Salva que este NPC já falou
        if (triggerOnlyOnce && !string.IsNullOrEmpty(uniqueId))
        {
            completedDialogues.Add(uniqueId);
        }

        dialogue_hud.instance.dialogueText.SetText("");
        dialogue_hud.instance.dialoguePanel.SetActive(false);

        pause_manager.SetPause(false);

        if (!isInterfaceCharacter)
        {
            if (PlayerReferences.Instance?.InteractionDetector != null)
            {
                PlayerReferences.Instance.InteractionDetector.ClearForcedInteractable(this);
            }
        }

        OnDialogueEnded?.Invoke();
        OnDialogueEnded = null;

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.InteractIcon.SetActive(true);

            if (PlayerReferences.Instance.playerMovement != null)
            {
                PlayerReferences.Instance.EnablePlayer();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!startDialogueOnTrigger || !other.CompareTag("Player") || hasTriggeredDialogue)
            return;

        if (triggerOnlyOnce && !string.IsNullOrEmpty(uniqueId) && completedDialogues.Contains(uniqueId))
            return;

        hasTriggeredDialogue = true;
        startDialogueOnTrigger = false;

        Collider2D areaCollider = GetComponent<Collider2D>();
        if (areaCollider != null)
        {
            areaCollider.enabled = false;
        }

        StartDialogueExternally();
    }
}
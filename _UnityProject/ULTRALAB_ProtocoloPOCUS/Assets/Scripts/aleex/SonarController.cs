using UnityEngine;
using TMPro;
using System.Collections;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Necessário para Image
using UnityEngine.SceneManagement;

public class SonarController : MonoBehaviour
{
    public static SonarController Instance { get; private set; }

    [Header("UI do Sonar")]
    [Tooltip("O painel pequeno que fica o tempo todo na tela.")]
    public GameObject idlePanel; 
    [Tooltip("O painel completo com a caixa de texto.")]
    public GameObject speakingPanel; 
    
    [Header("Textos e Imagens da Caixa de Fala")]
    public TMP_Text sonarDialogueText;
    public Image sonarPortraitImage;

    [Header("Configuração de FMOD (Opcional)")]
    public EventReference defaultVoiceEvent;

    private NPCdialogue currentDialogue;
    private int dialogueIndex;
    private bool isTyping;
    public bool isDialogueActive { get; private set; }
    
    private EventInstance voiceInstance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= HandleSceneUnloaded;
    }

    private void Start()
    {
        // Garante o estado inicial correto ao abrir o jogo
        SetSonarVisualState(false);
    }

    private void SetSonarVisualState(bool speaking)
    {
        if (idlePanel != null) idlePanel.SetActive(!speaking);
        if (speakingPanel != null) speakingPanel.SetActive(speaking);
    }

    public void SetIdleIconVisible(bool visible)
    {
        if (idlePanel == null)
            return;

        idlePanel.SetActive(visible && !isDialogueActive);
    }

    public void RestoreIdleIcon()
    {
        SetSonarVisualState(isDialogueActive);
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        // O Sonar permanece entre cenas; nunca deixe o painel de fala
        // persistir quando a cena que iniciou a fala foi descarregada.
        StopAllCoroutines();
        StopVoice();

        isTyping = false;
        isDialogueActive = false;
        currentDialogue = null;
        dialogueIndex = 0;

        if (sonarDialogueText != null) sonarDialogueText.SetText("");

        SetSonarVisualState(false);
        pause_manager.SetPause(false);

        if (PlayerReferences.Instance != null && PlayerReferences.Instance.playerMovement != null)
        {
            PlayerReferences.Instance.EnablePlayer();
        }
    }

    private void Update()
    {
        if (isDialogueActive)
        {
            bool pressed = false;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) pressed = true;
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)) pressed = true;

            if (pressed)
            {
                NextLine();
            }
        }
    }

    public void Falar(NPCdialogue falaDoSonar)
    {
        if (falaDoSonar == null || isDialogueActive) return;

        currentDialogue = falaDoSonar;
        isDialogueActive = true;
        dialogueIndex = 0;

        // O painel de fala substitui o ícone de fundo enquanto o Sonar fala.
        SetSonarVisualState(true);

        if (sonarPortraitImage != null) sonarPortraitImage.sprite = currentDialogue.npcPortrait;

        pause_manager.SetPause(true); 

        if (PlayerReferences.Instance != null && PlayerReferences.Instance.playerMovement != null)
        {
            PlayerReferences.Instance.DisablePlayer();
        }

        StartCoroutine(TypeLine());
    }

    public void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            StopVoice();
            if (sonarDialogueText != null) sonarDialogueText.SetText(currentDialogue.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < currentDialogue.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    public void CancelDialogueForSceneTransition()
    {
        StopAllCoroutines();
        StopVoice();

        isTyping = false;
        isDialogueActive = false;
        currentDialogue = null;
        dialogueIndex = 0;

        if (sonarDialogueText != null)
            sonarDialogueText.SetText("");

        SetSonarVisualState(false);
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        if (sonarDialogueText != null) sonarDialogueText.SetText("");

        StartVoice();

        foreach (char letter in currentDialogue.dialogueLines[dialogueIndex])
        {
            if (sonarDialogueText != null) sonarDialogueText.text += letter;
            yield return new WaitForSecondsRealtime(currentDialogue.typingSpeed);
        }

        StopVoice();
        isTyping = false;
    }

    private void StartVoice()
    {
        EventReference voiceToPlay = currentDialogue.voiceEvent.IsNull ? defaultVoiceEvent : currentDialogue.voiceEvent;
        if (voiceToPlay.IsNull) return;

        StopVoice();
        voiceInstance = RuntimeManager.CreateInstance(voiceToPlay);
        voiceInstance.start();
    }

    private void StopVoice()
    {
        if (voiceInstance.isValid())
        {
            voiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            voiceInstance.release();
            voiceInstance = default;
        }
    }

    private void EndDialogue()
    {
        StopAllCoroutines();
        StopVoice();

        isDialogueActive = false;
        currentDialogue = null;

        if (sonarDialogueText != null) sonarDialogueText.SetText("");
        
        // Ao terminar, o ícone com imagem e nome do Sonar volta a aparecer.
        SetSonarVisualState(false);

        pause_manager.SetPause(false);

        if (PlayerReferences.Instance != null && PlayerReferences.Instance.playerMovement != null)
        {
            PlayerReferences.Instance.EnablePlayer();
        }
    }
}

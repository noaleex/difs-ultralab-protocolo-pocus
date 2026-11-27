using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;

[System.Serializable]
public class CutsceneLine
{
    [Header("Configuração")]
    public bool isNarrator = true; 
    
    [Header("Dados do Personagem (Ignorado se for Narrador)")]
    [Tooltip("Marque se esta fala for do protagonista. Ele vai puxar o nome salvo do jogador.")]
    public bool isPlayerCharacter = false; // <-- NOVA OPÇÃO AQUI
    public string characterName;
    public Sprite characterPortrait; 
    
    [Header("Texto")]
    [TextArea(3, 5)] 
    public string text;
}

[System.Serializable]
public class CutsceneSlide
{
    [Header("Imagem de Fundo")]
    public Texture slideImage; 
    
    [Header("Atores e Animações (Opcional)")]
    public GameObject slideActors; 

    [Header("Falas deste Slide")]
    public List<CutsceneLine> lines;

    public EventReference slideSound;
}

public class CutsceneController : MonoBehaviour
{
    public static bool IsSkipKeyReserved { get; private set; }

    [Header("Sequência da Cutscene")]
    public List<CutsceneSlide> slides;
    public string nextSceneName = "gameplay_tutorial"; 

    [Header("UI Global")]
    public RawImage backgroundImage; 
    public RawImage blackScreen; 
    public float fadeSpeed = 1.5f;
    public float typingSpeed = 0.03f;

    [Header("Gesto Android")]
    [Tooltip("Tempo mÃ¡ximo entre os dois toques para considerar toque duplo.")]
    [SerializeField, Min(0.05f)] private float doubleTapInterval = 0.3f;
    [Tooltip("DistÃ¢ncia mÃ¡xima, em pixels, entre os dois toques.")]
    [SerializeField, Min(0f)] private float doubleTapMaxDistance = 120f;

    [Header("Som de DigitaÃ§Ã£o")]
    [Tooltip("Evento FMOD mantido enquanto as letras da fala estao sendo reveladas.")]
    [SerializeField] private EventReference typingSound;

    [Header("Sons de Fala de Personagem")]
    [Tooltip("Evento FMOD usado pela fala do enfermeiro na cutscene feminina.")]
    [SerializeField] private EventReference enfermeiroFemininoDialogueSound;
    [Tooltip("Evento FMOD usado pela fala do enfermeiro na cutscene masculina.")]
    [SerializeField] private EventReference enfermeiroMasculinoDialogueSound;
    [Tooltip("Evento FMOD usado pelas falas de Adrian.")]
    [SerializeField] private EventReference adrianDialogueSound;
    [Tooltip("Evento FMOD usado pelas falas do Sonar.")]
    [SerializeField] private EventReference sonarDialogueSound;

    [Header("UI do Narrador")]
    public GameObject narratorPanel;
    public TMP_Text narratorText;

    [Header("UI do Personagem")]
    public GameObject characterPanel;
    public TMP_Text characterText;
    public TMP_Text characterNameText;
    public Image characterPortrait;

    // Chave onde o nome do jogador é salvo ao criar o personagem
    private string chaveNomeJogador = "PlayerName"; 

    private bool isTyping = false;
    private bool skipTyping = false;
    private EventInstance slideSoundInstance;
    private Coroutine slideSoundCoroutine;
    private EventInstance textSoundInstance;
    private Coroutine textSoundCoroutine;
    private bool transitionStarted;

    private void OnEnable()
    {
        IsSkipKeyReserved = true;
        SkipTouchInput.Reset();
    }

    private void Start()
    {
        narratorPanel.SetActive(false);
        characterPanel.SetActive(false);
        
        foreach (var slide in slides)
        {
            if (slide.slideActors != null)
            {
                slide.slideActors.SetActive(false);
            }
        }

        StartCoroutine(PlayCutscene());
    }

    private void Update()
    {
        bool skipByKey = SkipKeyInput.WasPressedThisFrame();
        bool skipByDoubleTap = SkipTouchInput.WasDoubleTapThisFrame(
            doubleTapInterval,
            doubleTapMaxDistance);

        if (skipByKey || skipByDoubleTap)
        {
            TransitionToNextScene();
            return;
        }

        if (WasInputPressedThisFrame())
        {
            if (isTyping)
                skipTyping = true; 
        }
    }

    private void OnDestroy()
    {
        IsSkipKeyReserved = false;
        StopAllCoroutines();
        StopTextSound();
        StopSlideSound();
    }

    private void OnDisable()
    {
        IsSkipKeyReserved = false;
        SkipTouchInput.Reset();
    }

    private bool WasInputPressedThisFrame()
    {
        bool pressed = false;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) pressed = true;
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)) pressed = true;
        return pressed;
    }

    private IEnumerator PlayCutscene()
    {
        blackScreen.color = new Color(0, 0, 0, 1);
        blackScreen.gameObject.SetActive(true);

        for (int i = 0; i < slides.Count; i++)
        {
            CutsceneSlide currentSlide = slides[i];
            
            backgroundImage.texture = currentSlide.slideImage;
            
            if (currentSlide.slideActors != null) currentSlide.slideActors.SetActive(true);

            StartSlideSound(currentSlide.slideSound);
            yield return FadeBlackScreen(1, 0);

            foreach (CutsceneLine line in currentSlide.lines)
            {
                yield return PlayLine(line);
                
                skipTyping = false;
                while (!WasInputPressedThisFrame()) yield return null;
                yield return new WaitForSeconds(0.1f);
            }

            narratorPanel.SetActive(false);
            characterPanel.SetActive(false);

            StopTextSound();
            StopSlideSound();

            if (i < slides.Count - 1) yield return FadeBlackScreen(0, 1);

            if (currentSlide.slideActors != null) currentSlide.slideActors.SetActive(false);
        }

        yield return FadeBlackScreen(0, 1);
        
        TransitionToNextScene();
    }

    private void TransitionToNextScene()
    {
        if (transitionStarted)
            return;

        transitionStarted = true;
        isTyping = false;
        skipTyping = true;

        StopTextSound();
        StopSlideSound();
        StopAllCoroutines();

        if (narratorPanel != null)
            narratorPanel.SetActive(false);

        if (characterPanel != null)
            characterPanel.SetActive(false);

        foreach (CutsceneSlide slide in slides)
        {
            if (slide.slideActors != null)
                slide.slideActors.SetActive(false);
        }

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[CUTSCENE] loading_manager não encontrado!");
        }
    }

    private void StartSlideSound(EventReference sound)
    {
        StopSlideSound();

        if (sound.IsNull)
            return;

        slideSoundInstance = RuntimeManager.CreateInstance(sound);
        FMOD.RESULT result = slideSoundInstance.start();
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError($"[CUTSCENE] Falha ao iniciar áudio do slide: {result}", this);
            slideSoundInstance.release();
            slideSoundInstance.clearHandle();
            return;
        }

        slideSoundCoroutine = StartCoroutine(ReplaySlideSoundWhenFinished());
    }

    private IEnumerator ReplaySlideSoundWhenFinished()
    {
        while (slideSoundInstance.isValid())
        {
            PLAYBACK_STATE playbackState;
            FMOD.RESULT result = slideSoundInstance.getPlaybackState(out playbackState);
            if (result != FMOD.RESULT.OK)
            {
                Debug.LogError($"[CUTSCENE] Falha ao consultar áudio do slide: {result}", this);
                yield break;
            }

            if (playbackState == PLAYBACK_STATE.STOPPED)
            {
                result = slideSoundInstance.start();
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"[CUTSCENE] Falha ao reiniciar áudio do slide: {result}", this);
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void StopSlideSound()
    {
        if (slideSoundCoroutine != null)
        {
            StopCoroutine(slideSoundCoroutine);
            slideSoundCoroutine = null;
        }

        if (slideSoundInstance.isValid())
        {
            slideSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            slideSoundInstance.release();
            slideSoundInstance.clearHandle();
        }
    }

    private IEnumerator PlayLine(CutsceneLine line)
    {
        TMP_Text activeTextUI;

        StopTextSound();

        if (line.isNarrator)
        {
            narratorPanel.SetActive(true);
            characterPanel.SetActive(false);
            activeTextUI = narratorText;
        }
        else
        {
            narratorPanel.SetActive(false);
            characterPanel.SetActive(true);
            activeTextUI = characterText;
            
            if (characterNameText != null) 
            {
                // Se for o player, puxa o nome do save. Se não, usa o nome escrito no Inspector.
                if (line.isPlayerCharacter)
                {
                    characterNameText.text = PlayerPrefs.GetString(chaveNomeJogador, "Enfermeiro(a)");
                }
                else
                {
                    characterNameText.text = line.characterName;
                }
            }
            if (characterPortrait != null) characterPortrait.sprite = line.characterPortrait;

        }

        activeTextUI.text = "";
        isTyping = true;
        skipTyping = false;
        EventReference textSound = line.isNarrator
            ? typingSound
            : GetCharacterDialogueSound(line);
        StartTextSound(textSound);

        foreach (char letter in line.text)
        {
            if (skipTyping)
            {
                activeTextUI.text = line.text; 
                break;
            }

            activeTextUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        StopTextSound();
    }

    private EventReference GetCharacterDialogueSound(CutsceneLine line)
    {
        string characterName = line.isPlayerCharacter
            ? PlayerPrefs.GetString(chaveNomeJogador, "Enfermeiro(a)")
            : line.characterName;
        characterName = characterName ?? string.Empty;

        if (!line.isPlayerCharacter &&
            characterName.IndexOf("Adrian", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return adrianDialogueSound;
        }

        if (!line.isPlayerCharacter &&
            characterName.IndexOf("Sonar", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return sonarDialogueSound;
        }

        // "Enfermeiro" aparece nas duas versões; o nome sozinho não decide o áudio.
        if (line.isPlayerCharacter ||
            characterName.IndexOf("enfermeiro", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return IsFemaleCharacterSelected()
                ? enfermeiroFemininoDialogueSound
                : enfermeiroMasculinoDialogueSound;
        }

        return default;
    }

    private bool IsFemaleCharacterSelected()
    {
        // A escolha feita no menu Ã© salva imediatamente em PlayerPrefs("Character")
        // e Ã© a mesma fonte usada por CharacterSpawn para criar o jogador correto.
        // O save JSON Ã© apenas fallback, pois pode conter o gÃªnero da partida anterior
        // durante a transiÃ§Ã£o para a cutscene.
        string selectedGender = PlayerPrefs.GetString("Character", "");

        if (string.IsNullOrEmpty(selectedGender))
        {
            selectedGender = PlayerPrefs.GetString("PlayerGender", "");
        }

        if (string.IsNullOrEmpty(selectedGender) &&
            save_manager.instance != null &&
            save_manager.instance.currentData != null)
        {
            selectedGender = save_manager.instance.currentData.characterGender;
        }

        if (string.Equals(selectedGender, "Feminino", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(selectedGender, "Female", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(selectedGender, "Masculino", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(selectedGender, "Male", System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Fallback apenas para testar a cena isoladamente, sem o boot persistente.
        return SceneManager.GetActiveScene().name == "PF0_Intro";
    }

    private void StartTextSound(EventReference sound)
    {
        StopTextSound();

        if (sound.IsNull)
            return;

        textSoundInstance = RuntimeManager.CreateInstance(sound);
        FMOD.RESULT result = textSoundInstance.start();
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError($"[CUTSCENE] Falha ao iniciar som da fala: {result}", this);
            textSoundInstance.release();
            textSoundInstance.clearHandle();
            return;
        }

        textSoundCoroutine = StartCoroutine(ReplayTextSoundWhenFinished());
    }

    private IEnumerator ReplayTextSoundWhenFinished()
    {
        while (textSoundInstance.isValid() && isTyping)
        {
            PLAYBACK_STATE playbackState;
            FMOD.RESULT result = textSoundInstance.getPlaybackState(out playbackState);
            if (result != FMOD.RESULT.OK)
            {
                Debug.LogError($"[CUTSCENE] Falha ao consultar som da fala: {result}", this);
                yield break;
            }

            if (playbackState == PLAYBACK_STATE.STOPPED)
            {
                result = textSoundInstance.start();
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"[CUTSCENE] Falha ao reiniciar som da fala: {result}", this);
                    yield break;
                }
            }

            yield return null;
        }

        textSoundCoroutine = null;
    }

    private void StopTextSound()
    {
        if (textSoundCoroutine != null)
        {
            StopCoroutine(textSoundCoroutine);
            textSoundCoroutine = null;
        }

        if (!textSoundInstance.isValid())
            return;

        textSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        textSoundInstance.release();
        textSoundInstance.clearHandle();
    }

    private IEnumerator FadeBlackScreen(float startAlpha, float targetAlpha)
    {
        float timer = 0;
        Color c = blackScreen.color;

        while (timer < fadeSpeed)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeSpeed);
            blackScreen.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        blackScreen.color = c;
    }
}

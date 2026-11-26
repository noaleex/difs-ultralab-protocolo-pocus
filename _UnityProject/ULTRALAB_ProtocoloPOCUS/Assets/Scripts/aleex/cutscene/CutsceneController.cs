using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [Header("Sequência da Cutscene")]
    public List<CutsceneSlide> slides;
    public string nextSceneName = "gameplay_tutorial"; 

    [Header("UI Global")]
    public RawImage backgroundImage; 
    public RawImage blackScreen; 
    public float fadeSpeed = 1.5f;
    public float typingSpeed = 0.03f;

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
        if (WasInputPressedThisFrame())
        {
            if (isTyping)
                skipTyping = true; 
        }
    }

    private void OnDestroy()
    {
        StopSlideSound();
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

            StopSlideSound();

            if (i < slides.Count - 1) yield return FadeBlackScreen(0, 1);

            if (currentSlide.slideActors != null) currentSlide.slideActors.SetActive(false);
        }

        yield return FadeBlackScreen(0, 1);
        
        if (loading_manager.instance != null) loading_manager.instance.SwitchScene(nextSceneName);
        else Debug.LogWarning("[CUTSCENE] loading_manager não encontrado!");
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
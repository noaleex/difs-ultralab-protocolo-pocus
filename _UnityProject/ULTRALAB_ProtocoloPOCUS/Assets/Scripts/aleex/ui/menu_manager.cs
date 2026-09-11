using UnityEngine;
using UnityEngine.UI;
using UnityEditor; 
using FMODUnity;
using FMOD.Studio;
using TMPro;

public class menu_manager : MonoBehaviour
{
    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelCharacterSelection;
    [SerializeField] private GameObject panelMenuOptionsPc;
    [SerializeField] private GameObject panelMenuOptionsAndroid;
    [SerializeField] private GameObject panelMenuOutConfirmation;

    [SerializeField] private Button buttonContinue;

    [SerializeField] private Button buttonMale;
    [SerializeField] private Button buttonFemale;
    [SerializeField] private GameObject spriteMale;
    [SerializeField] private GameObject spriteFemale;

    [SerializeField] private Button buttonAcademic;
    [SerializeField] private Button buttonGraduated;

    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorSelected = Color.green;

    [SerializeField] private Button buttonConfirmSelection;

    [SerializeField] private TMP_InputField inputName;
    [SerializeField] private TMP_InputField inputAge;

    [SerializeField] private bool skipIntroCutscene = true;
    [SerializeField] private string sceneGameplayTutorial = "gameplay_tutorial";
    [SerializeField] private string sceneCutsceneMale = "PM0_Intro";
    [SerializeField] private string sceneCutsceneFemale = "PF0_Intro";

    public EventReference MusicaMenu;    
    public EventReference ClickMenu;
    public EventReference SelecionarPersonagemClick;
    private EventInstance menuMusicInstance;

    private string selectedCharacter = "";
    private string selectedDegree = "";

    private void Start()
    {
        if (!MusicaMenu.IsNull)
        {
            menuMusicInstance = RuntimeManager.CreateInstance(MusicaMenu);
            menuMusicInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            menuMusicInstance.start();
        }

        ReturnToMainMenu();
        CheckSave();
    }

    private void OnDestroy()
    {
        StopMenuMusic();
        if (menuMusicInstance.isValid())
        {
            menuMusicInstance.release();
        }
    }

    private void StopMenuMusic()
    {
        if (menuMusicInstance.isValid())
        {
            menuMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    private void PlayClickSound()
    {
        if (!ClickMenu.IsNull)
        {
            RuntimeManager.PlayOneShot(ClickMenu, transform.position);
        }
    }

    private void CheckSave()
    {
        bool hasValidSave = save_manager.instance != null && 
                            save_manager.instance.currentData != null && 
                            !string.IsNullOrEmpty(save_manager.instance.currentData.currentScene) &&
                            save_manager.instance.currentData.currentScene != "MainMenu" &&
                            save_manager.instance.currentData.currentScene != "Menu";

        if (buttonContinue != null)
        {
            buttonContinue.interactable = hasValidSave;
        }
    }

    public void Play()
    {
        PlayClickSound();
        panelMainMenu.SetActive(false);
        panelCharacterSelection.SetActive(true);

        selectedCharacter = "";
        selectedDegree = "";

        if (spriteMale != null) spriteMale.SetActive(false);
        if (spriteFemale != null) spriteFemale.SetActive(false);

        ResetSelectionVisuals();
        ValidateConfirmButton();
    }

    public void Back()
    {
        PlayClickSound();
        ReturnToMainMenu();
    }

    public void ReturnToMainMenu()
    {
        if (panelMainMenu != null) panelMainMenu.SetActive(true);
        if (panelCharacterSelection != null) panelCharacterSelection.SetActive(false);
        if (panelMenuOptionsPc != null) panelMenuOptionsPc.SetActive(false);
        if (panelMenuOptionsAndroid != null) panelMenuOptionsAndroid.SetActive(false);
        if (panelMenuOutConfirmation != null) panelMenuOutConfirmation.SetActive(false);

        selectedCharacter = "";
        selectedDegree = "";
        ResetSelectionVisuals();
    }

    public void ButtonMale()
    {
        if (!SelecionarPersonagemClick.IsNull)
        {
            RuntimeManager.PlayOneShot(SelecionarPersonagemClick, transform.position);
        }
        selectedCharacter = "Masculino";

        if (spriteMale != null) spriteMale.SetActive(true);
        if (spriteFemale != null) spriteFemale.SetActive(false);

        SetButtonColor(buttonMale, colorSelected);
        SetButtonColor(buttonFemale, colorNormal);

        ValidateConfirmButton();
    }

    public void ButtonFemale()
    {
        if (!SelecionarPersonagemClick.IsNull)
        {
            RuntimeManager.PlayOneShot(SelecionarPersonagemClick, transform.position);
        }
        selectedCharacter = "Feminino";

        if (spriteMale != null) spriteMale.SetActive(false);
        if (spriteFemale != null) spriteFemale.SetActive(true);

        SetButtonColor(buttonMale, colorNormal);
        SetButtonColor(buttonFemale, colorSelected);

        ValidateConfirmButton();
    }

    public void ButtonAcademic()
    {
        PlayClickSound();
        selectedDegree = "Academico";

        SetButtonColor(buttonAcademic, colorSelected);
        SetButtonColor(buttonGraduated, colorNormal);

        ValidateConfirmButton();
    }

    public void ButtonGraduated()
    {
        PlayClickSound();
        selectedDegree = "Formado";

        SetButtonColor(buttonAcademic, colorNormal);
        SetButtonColor(buttonGraduated, colorSelected);

        ValidateConfirmButton();
    }

    private void SetButtonColor(Button btn, Color targetColor)
    {
        if (btn == null) return;
        ColorBlock colors = btn.colors;
        colors.normalColor = targetColor;
        colors.selectedColor = targetColor;
        btn.colors = colors;
    }

    private void ResetSelectionVisuals()
    {
        SetButtonColor(buttonMale, colorNormal);
        SetButtonColor(buttonFemale, colorNormal);
        SetButtonColor(buttonAcademic, colorNormal);
        SetButtonColor(buttonGraduated, colorNormal);
    }

    private void ValidateConfirmButton()
    {
        if (buttonConfirmSelection != null)
        {
            buttonConfirmSelection.interactable = !string.IsNullOrEmpty(selectedCharacter) && 
                                                  !string.IsNullOrEmpty(selectedDegree);
        }
    }

    public void StartGame()
    {
        PlayClickSound();
        if (string.IsNullOrEmpty(selectedCharacter) || string.IsNullOrEmpty(selectedDegree))
        {
            return;
        }

        if (save_manager.instance != null)
        {
            save_manager.instance.currentData.characterGender = selectedCharacter;
            save_manager.instance.currentData.playerDegree = selectedDegree;
            if (inputName != null) save_manager.instance.currentData.playerName = inputName.text;
            if (inputAge != null) save_manager.instance.currentData.playerAge = inputAge.text;
            save_manager.instance.SaveGame();
        }

        StopMenuMusic();

        string targetScene = "";

        if (skipIntroCutscene)
        {
            targetScene = sceneGameplayTutorial;
        }
        else
        {
            targetScene = selectedCharacter == "Masculino" ? sceneCutsceneMale : sceneCutsceneFemale;
        }

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(targetScene);
        }
    }

    public void ContinueGame()
    {
        PlayClickSound();
        if (save_manager.instance != null && !string.IsNullOrEmpty(save_manager.instance.currentData.currentScene))
        {
            StopMenuMusic();
            if (loading_manager.instance != null)
            {
                loading_manager.instance.SwitchScene(save_manager.instance.currentData.currentScene);
            }
        }
    }

    public void Options()
    {
        PlayClickSound();
        panelMainMenu.SetActive(false);

        if (Application.platform == RuntimePlatform.Android)
        {
            panelMenuOptionsAndroid.SetActive(true);
        }
        else
        {
            panelMenuOptionsPc.SetActive(true);
        }
    }

    public void CloseOptions()
    {
        PlayClickSound();
        panelMenuOptionsPc.SetActive(false);
        panelMenuOptionsAndroid.SetActive(false);
        panelMainMenu.SetActive(true);
    }

    public void QuitConfirm()
    {
        PlayClickSound();
        panelMainMenu.SetActive(false);
        panelMenuOutConfirmation.SetActive(true);
    }

    public void QuitCancel()
    {
        PlayClickSound();
        panelMenuOutConfirmation.SetActive(false);
        panelMainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        PlayClickSound();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void AndroidConfirm()
    { 
        PlayClickSound();
        panelMenuOutConfirmation.SetActive(true);
        panelMainMenu.SetActive(false);
    }
}
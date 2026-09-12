using UnityEngine;
using UnityEngine.UI;

public class adrian_interaction : MonoBehaviour, IInteractable
{
    [SerializeField] private string dialogueCutsceneId = "adrian_intro";
    [SerializeField] private GameObject actionSelectionPanel;
    [SerializeField] private Button buttonApplyConduct;
    [SerializeField] private Button buttonStartExam;
    [SerializeField] private string examSceneName = "Exams";
    [SerializeField] private PatientData patientData;

    private bool dialogueFinished = false;

    private void Start()
    {
        if (actionSelectionPanel != null)
        {
            actionSelectionPanel.SetActive(false);
        }

        if (buttonApplyConduct != null)
        {
            buttonApplyConduct.interactable = false;
        }

        if (buttonStartExam != null)
        {
            buttonStartExam.onClick.RemoveAllListeners();
            buttonStartExam.onClick.AddListener(OnStartExamClicked);
        }
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        PauseController.SetPause(true);

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.DisablePlayer();
            if (PlayerReferences.Instance.InteractIcon != null)
            {
                PlayerReferences.Instance.InteractIcon.SetActive(false);
            }
        }

        if (text_cutscene_manager.instance != null)
        {
            text_cutscene_manager.instance.StartCutsceneDialogue(dialogueCutsceneId);
        }
        else
        {
            ShowActionMenu();
        }
    }

    public void ShowActionMenu()
    {
        if (actionSelectionPanel != null)
        {
            actionSelectionPanel.SetActive(true);
        }

        if (buttonApplyConduct != null)
        {
            buttonApplyConduct.interactable = false;
        }
    }

    public void OnStartExamClicked()
    {
        if (actionSelectionPanel != null)
        {
            actionSelectionPanel.SetActive(false);
        }

        if (patientData != null)
        {
            CurrentPatient.Data = patientData;
        }

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(examSceneName);
        }
    }

    public void CloseActionMenu()
    {
        if (actionSelectionPanel != null)
        {
            actionSelectionPanel.SetActive(false);
        }

        PauseController.SetPause(false);

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.EnablePlayer();
            if (PlayerReferences.Instance.InteractIcon != null)
            {
                PlayerReferences.Instance.InteractIcon.SetActive(true);
            }
        }
    }
}
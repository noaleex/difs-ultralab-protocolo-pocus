using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct cutscene_entry
{
    public string stepId;
    public string maleSceneName;
    public string femaleSceneName;
}

public class game_flow_manager : MonoBehaviour
{
    public static game_flow_manager instance { get; private set; }

    [SerializeField] private string gameplayTutorialSceneName = "gameplay_tutorial";
    [SerializeField] private List<cutscene_entry> cutscenes = new List<cutscene_entry>();

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

    public bool IsCutsceneScene(string sceneName)
    {
        foreach (cutscene_entry entry in cutscenes)
        {
            if (entry.maleSceneName == sceneName || entry.femaleSceneName == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    public void PlayCutscene(string stepId)
    {
        string gender = "";
        if (save_manager.instance != null && save_manager.instance.currentData != null)
        {
            gender = save_manager.instance.currentData.characterGender;
        }

        cutscene_entry entry = cutscenes.Find(x => x.stepId == stepId);

        string sceneToLoad = gender == "Feminino" ? entry.femaleSceneName : entry.maleSceneName;

        if (!string.IsNullOrEmpty(sceneToLoad) && loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(sceneToLoad);
        }
    }

    public void LoadGameplayTutorial()
    {
        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(gameplayTutorialSceneName);
        }
    }

    public void NotifyTutorialResult(bool approved)
    {
        if (approved)
        {
            PlayCutscene("approval");
        }
        else
        {
            PlayCutscene("reproval");
        }
    }
}
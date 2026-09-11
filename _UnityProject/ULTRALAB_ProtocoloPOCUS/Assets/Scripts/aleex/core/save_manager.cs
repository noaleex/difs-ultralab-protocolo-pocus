using UnityEngine;
using System.IO;

[System.Serializable]
public class save_data
{
    public string characterGender = "";
    public string playerName = "";
    public string playerAge = "";
    public string playerDegree = "";

    public string currentScene = "MainMenu";
    public int iccScore = 0;
    public bool hasFinishedTutorial = false;
}

public class save_manager : MonoBehaviour
{
    public static save_manager instance { get; private set; }
    public save_data currentData { get; private set; }

    private string saveFilePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "ultralab.prontuario.json");
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(saveFilePath, json);
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentData = JsonUtility.FromJson<save_data>(json);
        }
        else
        {
            currentData = new save_data();
        }
    }
}
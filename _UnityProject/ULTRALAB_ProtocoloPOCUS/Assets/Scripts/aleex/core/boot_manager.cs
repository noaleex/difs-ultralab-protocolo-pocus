using UnityEngine;
using UnityEngine.SceneManagement;

public class boot_manager : MonoBehaviour
{
    public static boot_manager instance { get; private set; }

    [SerializeField] private string initialSceneName = "MainMenu";
    [SerializeField] private Camera persistentCamera;

    [SerializeField] private GameObject malePlayerPrefab;
    [SerializeField] private GameObject femalePlayerPrefab;

    public GameObject activePlayer { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (persistentCamera == null)
        {
            persistentCamera = Camera.main;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        LoadInitialScene();
    }

    private void LoadInitialScene()
    {
        SceneManager.LoadScene(initialSceneName, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanupDuplicateCameras();
        HandlePlayerStateForScene(scene.name);
    }

    private void CleanupDuplicateCameras()
    {
        Camera[] camerasInHierarchy = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (Camera cam in camerasInHierarchy)
        {
            if (cam != persistentCamera)
            {
                Destroy(cam.gameObject);
            }
        }
    }

    private void HandlePlayerStateForScene(string sceneName)
    {
        bool isCutscene = game_flow_manager.instance != null && game_flow_manager.instance.IsCutsceneScene(sceneName);

        if (sceneName == initialSceneName || isCutscene)
        {
            if (activePlayer != null)
            {
                activePlayer.SetActive(false);
            }
            return;
        }

        EnsurePlayerInstantiated();

        if (activePlayer != null)
        {
            activePlayer.SetActive(true);

            GameObject spawnPoint = GameObject.FindWithTag("Respawn");
            if (spawnPoint != null)
            {
                activePlayer.transform.position = spawnPoint.transform.position;
            }
        }
    }

    public void EnsurePlayerInstantiated()
    {
        if (activePlayer != null)
        {
            return;
        }

        string savedGender = "";
        if (save_manager.instance != null && save_manager.instance.currentData != null)
        {
            savedGender = save_manager.instance.currentData.characterGender;
        }

        GameObject selectedPrefab = savedGender == "Feminino" ? femalePlayerPrefab : malePlayerPrefab;

        if (selectedPrefab != null)
        {
            activePlayer = Instantiate(selectedPrefab);
            DontDestroyOnLoad(activePlayer);
        }
    }

    public void RegisterPlayer(GameObject player)
    {
        activePlayer = player;
    }

    public bool HasPlayer()
    {
        return activePlayer != null;
    }
}
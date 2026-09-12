using Unity.Cinemachine;
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

    // Memória de posição do player
    private Vector3? storedPosition = null;
    private string storedPositionScene = "";

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

        if (persistentCamera != null)
        {
            DontDestroyOnLoad(persistentCamera.gameObject);
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
        HandleGameplayStateForScene(scene.name);
    }

    private void CleanupDuplicateCameras()
    {
        Camera[] camerasInHierarchy = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        foreach (Camera cam in camerasInHierarchy)
        {
            if (cam != persistentCamera && !cam.gameObject.CompareTag("MainCamera"))
            {
                Destroy(cam.gameObject);
            }
            else if (cam != persistentCamera && cam.gameObject.CompareTag("MainCamera") && persistentCamera != null)
            {
                Destroy(cam.gameObject);
            }
        }
    }

    private void HandleGameplayStateForScene(string sceneName)
    {
        bool isMenu = string.Equals(sceneName, initialSceneName, System.StringComparison.OrdinalIgnoreCase);
        bool isCutscene = game_flow_manager.instance != null && game_flow_manager.instance.IsCutsceneScene(sceneName);

        if (isMenu || isCutscene)
        {
            if (activePlayer != null)
            {
                activePlayer.SetActive(false);
            }

            if (hud_manager.instance != null)
            {
                hud_manager.instance.SetHudVisibility(false);
            }
            return;
        }

        EnsurePlayerInstantiated();

        if (activePlayer != null)
        {
            activePlayer.SetActive(true);

            // 1. Garante que os scripts de movimento sejam religados
            PlayerReferences refs = activePlayer.GetComponent<PlayerReferences>();
            if (refs != null)
            {
                refs.EnablePlayer();
            }

            // 2. Decide onde o player vai aparecer (Posição Salva ou Tag Respawn)
            if (storedPosition.HasValue && storedPositionScene == sceneName)
            {
                activePlayer.transform.position = storedPosition.Value;
                // Limpa a memória após usar para não travar o jogador aqui para sempre
                storedPosition = null;
                storedPositionScene = "";
            }
            else
            {
                GameObject spawnPoint = GameObject.FindWithTag("Respawn");
                if (spawnPoint != null)
                {
                    activePlayer.transform.position = spawnPoint.transform.position;
                }
            }

            BindCameraToPlayer(activePlayer.transform);
        }

        if (hud_manager.instance != null)
        {
            hud_manager.instance.SetHudVisibility(true);
        }
    }

    private void BindCameraToPlayer(Transform playerTransform)
    {
        CinemachineCamera vcam = FindAnyObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Target.TrackingTarget = playerTransform;
        }
    }

    public void StorePlayerPosition(Vector3 position, string sceneName)
    {
        storedPosition = position;
        storedPositionScene = sceneName;
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

        if (string.IsNullOrEmpty(savedGender))
        {
            return;
        }

        GameObject selectedPrefab = savedGender == "Feminino" ? femalePlayerPrefab : malePlayerPrefab;

        if (selectedPrefab != null)
        {
            activePlayer = Instantiate(selectedPrefab);
            DontDestroyOnLoad(activePlayer);

            PlayerReferences references = activePlayer.GetComponent<PlayerReferences>();
            if (references != null)
            {
                references.RefreshReferences();
                if (AndroidControl.Instance != null)
                {
                    AndroidControl.Instance.SetPlayerInteraction(references.InteractionDetector);
                }
            }
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
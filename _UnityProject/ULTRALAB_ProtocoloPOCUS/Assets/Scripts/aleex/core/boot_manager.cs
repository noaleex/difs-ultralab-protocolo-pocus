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

    private Vector3? storedPosition = null;
    private string storedPositionScene = "";
    private string activePlayerGender = "";

    private string pendingTutorialSpawnScene = "";

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

    private void LateUpdate()
    {
        if (persistentCamera == null ||
            activePlayer == null ||
            !activePlayer.activeInHierarchy)
        {
            return;
        }

        if (FindAnyObjectByType<CinemachineCamera>() != null)
        {
            return;
        }

        Vector3 cameraPosition = persistentCamera.transform.position;
        cameraPosition.x = activePlayer.transform.position.x;
        cameraPosition.y = activePlayer.transform.position.y;
        persistentCamera.transform.position = cameraPosition;
    }

    private void LoadInitialScene()
    {
        SceneManager.LoadScene(initialSceneName, LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanupDuplicateCameras();
        HandleGameplayStateForScene(scene);
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

    private void HandleGameplayStateForScene(Scene scene)
    {
        string sceneName = scene.name;

        bool isMenu = string.Equals(
            sceneName,
            initialSceneName,
            System.StringComparison.OrdinalIgnoreCase);

        bool isCutscene =
            game_flow_manager.instance != null &&
            game_flow_manager.instance.IsCutsceneScene(sceneName);

        if (isMenu || isCutscene)
        {
            if (activePlayer != null)
                activePlayer.SetActive(false);

            if (hud_manager.instance != null)
                hud_manager.instance.SetHudVisibility(false);

            return;
        }

        EnsurePlayerInstantiated();

        if (activePlayer != null)
        {
            activePlayer.SetActive(true);

            PlayerReferences refs =
                activePlayer.GetComponent<PlayerReferences>();

            if (refs != null)
                refs.EnablePlayer();

            // Prioridade: ponto de saída do tutorial.
            if (pendingTutorialSpawnScene == sceneName)
            {
                TutorialExitSpawn tutorialSpawn =
                    GetTutorialSpawnFromScene(scene);

                if (tutorialSpawn != null)
                {
                    activePlayer.transform.SetPositionAndRotation(
                        tutorialSpawn.transform.position,
                        tutorialSpawn.transform.rotation);
                }
                else
                {
                    Debug.LogWarning(
                        "TutorialExitSpawn não encontrado na cena " + sceneName);
                }

                pendingTutorialSpawnScene = "";
                storedPosition = null;
                storedPositionScene = "";
            }
            else if (storedPosition.HasValue &&
                     storedPositionScene == sceneName)
            {
                activePlayer.transform.position = storedPosition.Value;

                storedPosition = null;
                storedPositionScene = "";
            }
            else
            {
                Transform spawnPoint = GetRespawnFromScene(scene);

                if (spawnPoint != null)
                {
                    activePlayer.transform.SetPositionAndRotation(
                        spawnPoint.position,
                        spawnPoint.rotation);
                }
            }

            BindCameraToPlayer(activePlayer.transform);
        }

        if (hud_manager.instance != null)
        {
            hud_manager.instance.SetHudVisibility(true);
            hud_manager.instance.UpdateContextualHud(sceneName);
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

    public void PrepareForNewGame(string selectedGender)
    {
        if (selectedGender != "Feminino" && selectedGender != "Masculino")
        {
            Debug.LogWarning($"Gênero inválido para uma nova partida: '{selectedGender}'.");
            return;
        }

        // O alimento pertence à partida atual e não pode vazar para uma nova.
        FoodInventory.Clear();

        // A nova partida começa com o personagem escolhido no menu.
        if (activePlayer != null)
        {
            if (GameManager.Instance != null && GameManager.Instance.Player == activePlayer)
            {
                GameManager.Instance.RegisterPlayer(null);
            }

            Destroy(activePlayer);
            activePlayer = null;
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(null);
        }

        activePlayerGender = "";

        // Não reaproveitar a posição de uma partida anterior.
        storedPosition = null;
        storedPositionScene = "";

        PlayerPrefs.SetString("Character", selectedGender);
        PlayerPrefs.SetString(
            "PlayerGender",
            selectedGender == "Feminino" ? "Female" : "Male");

        if (save_manager.instance != null &&
            save_manager.instance.currentData != null)
        {
            save_manager.instance.currentData.characterGender = selectedGender;
            save_manager.instance.SaveGame();
        }

        PlayerPrefs.Save();
    }

    public void EnsurePlayerInstantiated()
    {
        string selectedGender = GetSelectedGender();

        if (string.IsNullOrEmpty(selectedGender))
        {
            Debug.LogWarning(
                "Não foi possível criar o jogador: nenhum personagem foi selecionado.",
                this);
            return;
        }

        // Reaproveita primeiro qualquer jogador que já tenha sobrevivido à troca
        // de cena, inclusive quando o boot_manager foi recriado.
        if (activePlayer == null)
            activePlayer = FindExistingPlayer();

        if (activePlayer != null)
        {
            // Se o jogador persistente já representa a escolha atual, não cria
            // outro objeto. Caso o gênero conhecido seja diferente, substitui-o.
            if (!PlayerMatchesGender(activePlayer, selectedGender))
            {
                Destroy(activePlayer);
                activePlayer = null;
            }
            else
            {
                activePlayerGender = selectedGender;
                RegisterActivePlayer(selectedGender);
                return;
            }
        }

        GameObject selectedPrefab = selectedGender == "Feminino"
            ? femalePlayerPrefab
            : malePlayerPrefab;

        if (selectedPrefab == null)
        {
            Debug.LogError(
                $"Prefab do personagem '{selectedGender}' não foi configurado no boot_manager.");
            return;
        }

        activePlayer = Instantiate(selectedPrefab);
        activePlayerGender = selectedGender;

        DontDestroyOnLoad(activePlayer);
        RegisterActivePlayer(selectedGender);
    }

    public void RegisterPlayer(GameObject player)
    {
        activePlayer = player;

        activePlayerGender = GetSelectedGender();
        RegisterActivePlayer(activePlayerGender);
    }

    public bool HasPlayer()
    {
        return activePlayer != null;
    }

    public void PrepareTutorialExitSpawn(string destinationSceneName)
    {
        if (string.IsNullOrWhiteSpace(destinationSceneName))
        {
            Debug.LogWarning("Cena de destino do tutorial não configurada.");
            return;
        }

        pendingTutorialSpawnScene = destinationSceneName;

        // Impede que uma posição anterior sobrescreva o spawn do tutorial.
        storedPosition = null;
        storedPositionScene = "";
    }

    private TutorialExitSpawn GetTutorialSpawnFromScene(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            TutorialExitSpawn spawn =
                root.GetComponentInChildren<TutorialExitSpawn>(true);

            if (spawn != null)
                return spawn;
        }

        return null;
    }

    private string GetSelectedGender()
    {
        string selectedGender = NormalizeGender(
            PlayerPrefs.GetString("Character", ""));

        if (string.IsNullOrEmpty(selectedGender) &&
            save_manager.instance != null &&
            save_manager.instance.currentData != null)
        {
            selectedGender = NormalizeGender(
                save_manager.instance.currentData.characterGender);
        }

        if (string.IsNullOrEmpty(selectedGender))
        {
            selectedGender = NormalizeGender(
                PlayerPrefs.GetString("PlayerGender", ""));
        }

        return selectedGender;
    }

    private string NormalizeGender(string value)
    {
        if (string.Equals(value, "Feminino", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "Female", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Feminino";
        }

        if (string.Equals(value, "Masculino", System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "Male", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Masculino";
        }

        return "";
    }

    private GameObject FindExistingPlayer()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            return GameManager.Instance.Player;
        }

        PlayerReferences[] references = FindObjectsByType<PlayerReferences>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (PlayerReferences reference in references)
        {
            if (reference != null && reference.gameObject != null)
                return reference.gameObject;
        }

        return null;
    }

    private bool PlayerMatchesGender(GameObject player, string selectedGender)
    {
        if (player == null)
            return false;

        if (!string.IsNullOrEmpty(activePlayerGender))
            return activePlayerGender == selectedGender;

        string playerName = player.name;

        if (playerName.IndexOf("Female", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return selectedGender == "Feminino";

        if (playerName.IndexOf("Male", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return selectedGender == "Masculino";

        // Prefabs personalizados sem o gênero no nome continuam sendo
        // reaproveitados para evitar a criação de uma segunda instância.
        return true;
    }

    private void RegisterActivePlayer(string selectedGender)
    {
        if (activePlayer == null)
            return;

        DontDestroyOnLoad(activePlayer);

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != activePlayer)
        {
            GameManager.Instance.RegisterPlayer(activePlayer);
        }

        PlayerReferences references =
            activePlayer.GetComponent<PlayerReferences>();

        if (references != null)
        {
            references.RefreshReferences();

            if (AndroidControl.Instance != null)
            {
                AndroidControl.Instance.SetPlayerInteraction(
                    references.InteractionDetector);
            }
        }

        activePlayerGender = selectedGender;
    }

    private Transform GetRespawnFromScene(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform candidate in transforms)
            {
                if (candidate.CompareTag("Respawn"))
                    return candidate;
            }
        }

        return null;
    }
}

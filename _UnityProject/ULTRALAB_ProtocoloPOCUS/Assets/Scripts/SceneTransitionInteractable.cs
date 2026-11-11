using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnId;

    public bool CanInteract()
    {
        return !string.IsNullOrWhiteSpace(targetScene) &&
               !string.IsNullOrWhiteSpace(targetSpawnId);
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        SceneTransitionSpawn.SetPending(targetScene, targetSpawnId);

        if (loading_manager.instance != null)
        {
            loading_manager.instance.SwitchScene(targetScene);
        }
        else
        {
            SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
        }
    }
}

public class SceneTransitionSpawn : MonoBehaviour
{
    [SerializeField] private string spawnId;

    private static string pendingScene;
    private static string pendingSpawnId;

    public static void SetPending(string sceneName, string spawnId)
    {
        pendingScene = sceneName;
        pendingSpawnId = spawnId;
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(spawnId) ||
            pendingScene != gameObject.scene.name ||
            pendingSpawnId != spawnId)
            return;

        pendingScene = null;
        pendingSpawnId = null;

        GameObject player = boot_manager.instance != null && boot_manager.instance.activePlayer != null
            ? boot_manager.instance.activePlayer
            : (GameManager.Instance != null ? GameManager.Instance.Player : null);

        if (player == null)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPlayer(player);

        player.transform.SetPositionAndRotation(transform.position, transform.rotation);

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
            body.linearVelocity = Vector2.zero;

        PlayerReferences references = player.GetComponent<PlayerReferences>();
        if (references != null)
            references.EnablePlayer();

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Unity.Cinemachine.CinemachineBrain brain =
                mainCamera.GetComponent<Unity.Cinemachine.CinemachineBrain>();

            if (brain != null)
            {
                Unity.Cinemachine.CinemachineCamera[] cameras =
                    Object.FindObjectsByType<Unity.Cinemachine.CinemachineCamera>(
                        FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None);

                foreach (Unity.Cinemachine.CinemachineCamera camera in cameras)
                    camera.Follow = player.transform;
            }
        }
    }
}

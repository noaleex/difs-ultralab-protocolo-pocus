using UnityEngine;

public class PlayerReferences : MonoBehaviour
{
    public static PlayerReferences Instance;

    [Header("Referências")]
    [SerializeField] private GameObject interactIcon;
    [SerializeField] private InteractionDetector interactionDetector;

    [Header("Movimento")]
    public CharacterController2D playerMovement;
    public PlayerMovementNavMash playerNavMesh;

    public GameObject InteractIcon => interactIcon;
    public InteractionDetector InteractionDetector => interactionDetector;

    private bool isAndroid;

    private void Awake()
    {
        Instance = this;
        isAndroid = Application.platform == RuntimePlatform.Android;
    }

    private void Start()
    {
        RefreshReferences();
    }

    public void RefreshReferences()
    {
        playerMovement = GetComponent<CharacterController2D>();
        playerNavMesh = GetComponent<PlayerMovementNavMash>();
        interactionDetector = GetComponentInChildren<InteractionDetector>();

        if (interactIcon == null)
        {
            GameObject icon = GameObject.Find("InteractIcon");
            if (icon != null)
                interactIcon = icon;
        }
    }

    public void EnablePlayer()
    {
        RefreshReferences();
        gameObject.SetActive(true);

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (!isAndroid)
        {
            return;
        }
        else
        {
            if (playerNavMesh != null)
            {
                playerNavMesh.enabled = true;
            }
        }
    }

    public void DisablePlayer()
    {
        if (playerNavMesh != null)
        {
            playerNavMesh.StopAnimation();
            playerNavMesh.enabled = false;
        }

        if (playerMovement != null)
            playerMovement.enabled = false;
    }
}
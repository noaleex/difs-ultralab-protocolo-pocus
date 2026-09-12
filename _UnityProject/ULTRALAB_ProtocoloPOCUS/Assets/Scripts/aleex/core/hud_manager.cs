using UnityEngine;

public class hud_manager : MonoBehaviour
{
    public static hud_manager instance { get; private set; }

    [SerializeField] private GameObject hudMasterContainer;
    [SerializeField] private GameObject mobileHudContainer;
    [SerializeField] private GameObject pcHudContainer;

    [SerializeField] private bool forceMobileInEditor = false;

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
            return;
        }

        SetupPlatformHud();
        SetHudVisibility(false);
    }

    private void SetupPlatformHud()
    {
        bool isMobile = Application.isMobilePlatform;

#if UNITY_EDITOR
        if (forceMobileInEditor)
        {
            isMobile = true;
        }
#endif

        if (mobileHudContainer != null)
        {
            mobileHudContainer.SetActive(isMobile);
        }

        if (pcHudContainer != null)
        {
            pcHudContainer.SetActive(!isMobile);
        }
    }

    public void SetHudVisibility(bool visible)
    {
        if (hudMasterContainer != null)
        {
            hudMasterContainer.SetActive(visible);
        }
    }
}
using UnityEngine;
using System;

public class hud_manager : MonoBehaviour
{
    public static hud_manager instance { get; private set; }

    [Header("Containers Principais")]
    [SerializeField] private GameObject hudMasterContainer;
    [SerializeField] private GameObject mobileHudContainer;
    [SerializeField] private GameObject pcHudContainer;

    [Header("Elementos Contextuais")]
    [SerializeField] private GameObject timerContainer;
    [SerializeField] private string[] scenesWithoutTimer;

    [Header("Configurações")]
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

    public void UpdateContextualHud(string sceneName)
    {
        if (timerContainer != null)
        {
            bool hideTimer = false;
            
            if (scenesWithoutTimer != null)
            {
                foreach (string s in scenesWithoutTimer)
                {
                    if (!string.IsNullOrEmpty(s) && string.Equals(s.Trim(), sceneName.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        hideTimer = true;
                        break;
                    }
                }
            }

            timerContainer.SetActive(!hideTimer);
            Debug.Log($"[HUD_MANAGER] Avaliando cena: '{sceneName}'. Relógio oculto? {hideTimer}");
        }
    }
}
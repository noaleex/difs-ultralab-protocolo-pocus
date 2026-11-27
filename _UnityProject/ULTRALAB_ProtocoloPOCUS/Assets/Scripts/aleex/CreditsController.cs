using UnityEngine;
using UnityEngine.InputSystem;

public class CreditsController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private RectTransform creditsContent;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 80f;
    [Tooltip("Extra distance above the screen before closing automatically.")]
    [SerializeField] private float endPadding = 150f;

    private bool isPlaying;
    private float startPosY;
    private float endPosY;

    private void Awake()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying) return;

        // Allow skipping with Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseCredits();
            return;
        }

        // Scroll upward using unscaledDeltaTime (works even if game is paused)
        creditsContent.anchoredPosition += Vector2.up * (scrollSpeed * Time.unscaledDeltaTime);

        // Close automatically when the entire content passes the top of the screen
        if (creditsContent.anchoredPosition.y >= endPosY)
        {
            CloseCredits();
        }
    }

    public void OpenCredits()
    {
        if (creditsPanel == null || creditsContent == null) return;

        creditsPanel.SetActive(true);

        // Force layout rebuild so rect.height is accurate before calculating positions
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(creditsContent);

        RectTransform panelRect = creditsPanel.GetComponent<RectTransform>();
        float screenHeight = panelRect.rect.height;

        // Start just below the bottom edge (assuming Pivot Y = 1 and Anchor = Bottom)
        startPosY = 0f;
        creditsContent.anchoredPosition = new Vector2(0f, startPosY);

        // End when the bottom of the content clears the top of the screen
        endPosY = screenHeight + creditsContent.rect.height + endPadding;

        isPlaying = true;
    }

    public void CloseCredits()
    {
        isPlaying = false;

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }
}
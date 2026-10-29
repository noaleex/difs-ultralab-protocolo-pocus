using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasPageNavigation : MonoBehaviour
{
    [Header("Páginas")]
    [SerializeField] private GameObject[] pages;
    [SerializeField] private int currentPageIndex = 0;

    [Header("Input System")]
    [SerializeField] private InputActionReference upPageAction;
    [SerializeField] private InputActionReference downPageAction;


    // =========================================================
    // INPUT SYSTEM
    // =========================================================

    private void OnEnable()
    {
        if (upPageAction != null)
        {
            upPageAction.action.performed += OnUpPagePressed;
            upPageAction.action.Enable();
        }

        if (downPageAction != null)
        {
            downPageAction.action.performed += OnDownPagePressed;
            downPageAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (upPageAction != null)
        {
            upPageAction.action.performed -= OnUpPagePressed;
            upPageAction.action.Disable();
        }

        if (downPageAction != null)
        {
            downPageAction.action.performed -= OnDownPagePressed;
            downPageAction.action.Disable();
        }
    }

    private void OnUpPagePressed(InputAction.CallbackContext context)
    {
        UpPage();
    }

    private void OnDownPagePressed(InputAction.CallbackContext context)
    {
        DownPage();
    }


    // =========================================================
    // PÁGINAS
    // =========================================================

    public void UpPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        if (currentPageIndex < 0 ||
            currentPageIndex >= pages.Length)
        {
            currentPageIndex = 0;
        }

        if (pages[currentPageIndex] != null)
            pages[currentPageIndex].SetActive(false);

        currentPageIndex = (currentPageIndex + 1) % pages.Length;

        if (pages[currentPageIndex] != null)
            pages[currentPageIndex].SetActive(true);
    }


    public void DownPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        if (currentPageIndex < 0 ||
            currentPageIndex >= pages.Length)
        {
            currentPageIndex = 0;
        }

        if (pages[currentPageIndex] != null)
            pages[currentPageIndex].SetActive(false);

        currentPageIndex = (currentPageIndex - 1 + pages.Length) % pages.Length;

        if (pages[currentPageIndex] != null)
            pages[currentPageIndex].SetActive(true);
    }
}

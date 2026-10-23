using UnityEngine;

public class tutorial_menu_controller : MonoBehaviour
{
    [SerializeField] private GameObject tutorialProntuarioPanel;

    private void OnEnable()
    {
        // Rouba a prioridade do botão ESC para este script
        pause_manager.overridePauseAction += ToggleMenu;
    }

    private void OnDisable()
    {
        // Devolve a prioridade ao sair da cena ou desativar o painel
        pause_manager.overridePauseAction -= ToggleMenu;
    }

    public void ToggleMenu()
    {
        if (tutorialProntuarioPanel != null)
        {
            if (tutorialProntuarioPanel.activeSelf)
                CloseMenu();
            else
                OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (tutorialProntuarioPanel != null)
            tutorialProntuarioPanel.SetActive(true);

        pause_manager.SetPause(true);
    }

    public void CloseMenu()
    {
        if (tutorialProntuarioPanel != null)
            tutorialProntuarioPanel.SetActive(false);

        pause_manager.SetPause(false);
    }
}
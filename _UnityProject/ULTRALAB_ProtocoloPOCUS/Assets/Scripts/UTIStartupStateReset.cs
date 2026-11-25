using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
public sealed class UTIStartupStateReset : MonoBehaviour
{
    private void Awake()
    {
        // The pause_manager persists from _boot and can carry a stale pause
        // from a previous menu/dialogue. UTI starts as active gameplay.
        PauseController.SetPause(false);

        if (pause_manager.instance != null)
            pause_manager.SetPause(false);
        else
            Time.timeScale = 1f;

        HideDialoguePanelGraphic();
        StartCoroutine(HideDialoguePanelGraphicAfterStartup());
    }

    private IEnumerator HideDialoguePanelGraphicAfterStartup()
    {
        yield return null;
        yield return null;
        HideDialoguePanelGraphic();
    }

    private void HideDialoguePanelGraphic()
    {
        // A instância local do balão de diálogo pode deixar um mesh antigo
        // desenhado durante a troca de cena. Oculta somente o gráfico desse
        // painel e força a atualização do Canvas; o cronômetro permanece
        // intacto.
        foreach (Image image in FindObjectsByType<Image>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (image.gameObject.name != "DialoguePanel" ||
                image.gameObject.scene != gameObject.scene)
                continue;

            image.color = new Color(1f, 1f, 1f, 0f);
            image.enabled = false;
            image.gameObject.SetActive(false);
        }

        Canvas.ForceUpdateCanvases();
    }
}

using UnityEngine;
using FMODUnity;

public class ToolsButton : MonoBehaviour
{
    public ToolsSelected.ToolsType toolType;

    [Header("FMOD")]
    public EventReference clickSound;

    public void SelectTool()
    {
       
        if (!clickSound.IsNull)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShot(clickSound);
            }
            else
            {
                RuntimeManager.PlayOneShot(clickSound);
            }
        }

        if (ToolsManager.Instance != null)
            ToolsManager.Instance.SelectTool(toolType);
        else
            Debug.LogError("ToolsManager não está presente na cena.");
    }
}
using UnityEngine;
using FMODUnity;

public class ButtonBodyArea : MonoBehaviour
{
    public enum BodyRegion
    {
        Hands,
        Elbows,
        Legs
    }

    public BodyRegion region;

    [Header("FMOD")]
    public EventReference clickSound;

    public void OnClick()
    {
        if (!clickSound.IsNull)
        {
            RuntimeManager.PlayOneShot(clickSound);
        }

        PalpationManager.Instance.AddInfo(region);
    }
}
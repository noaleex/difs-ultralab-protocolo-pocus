using UnityEngine;

public class SkipDay : MonoBehaviour, IInteractable
{
    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (Timer.instance != null)
        {
            Timer.instance.TentarDormir();
        }
    }
}
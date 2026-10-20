using UnityEngine;

public class SkipDay : MonoBehaviour, IInteractable
{
    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (Timer.Instance == null)
        {
            Debug.LogError("[SkipDay] Timer.Instance não está disponível na cena.", this);
            return;
        }

        Timer.Instance.SkipToNextDay();
    }
}
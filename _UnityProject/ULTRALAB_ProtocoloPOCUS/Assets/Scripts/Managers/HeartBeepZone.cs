using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(Collider2D))]
public class HeartBeepZone : MonoBehaviour
{
    [Header("FMOD - Batimento da zona_bip")]
    [Tooltip("Evento sustentado enquanto o jogador estiver dentro da zona.")]
    [SerializeField] private EventReference heartBeep;

    private EventInstance heartInstance;
    private bool playerInside;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerInside || heartInstance.isValid())
            return;

        if (heartBeep.IsNull)
        {
            Debug.LogError("[HeartBeepZone] O evento FMOD não foi atribuído no Inspector.", this);
            return;
        }

        heartInstance = RuntimeManager.CreateInstance(heartBeep);
        FMOD.RESULT result = heartInstance.start();
        if (result != FMOD.RESULT.OK)
        {
            heartInstance.release();
            heartInstance.clearHandle();
            return;
        }

        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        StopHeart();
    }

    private void OnDisable()
    {
        playerInside = false;
        StopHeart();
    }

    private void OnDestroy()
    {
        StopHeart();
    }

    private void StopHeart()
    {
        if (!heartInstance.isValid())
            return;

        heartInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        heartInstance.release();
        heartInstance.clearHandle();
    }
}

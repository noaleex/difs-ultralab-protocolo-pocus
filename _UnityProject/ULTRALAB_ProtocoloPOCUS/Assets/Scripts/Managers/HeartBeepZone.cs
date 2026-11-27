using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class HeartBeepZone : MonoBehaviour
{
    [Header("FMOD - Batimento")]
    [Tooltip("Evento FMOD reproduzido enquanto o jogador estiver dentro da zona.")]
    [SerializeField] private EventReference heartBeep;

    private EventInstance heartInstance;
    private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayerCollider(other) || !playerColliders.Add(other))
            return;

        if (playerColliders.Count > 1 || heartInstance.isValid())
            return;

        if (heartBeep.IsNull)
        {
            Debug.LogError("[HeartBeepZone] O evento fx_heart_beep não foi configurado no Inspector.", this);
            return;
        }

        heartInstance = RuntimeManager.CreateInstance(heartBeep);

        FMOD.RESULT result = heartInstance.start();
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError($"[HeartBeepZone] Falha ao iniciar fx_heart_beep: {result}", this);
            heartInstance.release();
            heartInstance.clearHandle();
            heartInstance = default;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayerCollider(other) || !playerColliders.Remove(other))
            return;

        if (playerColliders.Count == 0)
            StopHeartBeep();
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        return other != null &&
               (other.CompareTag("Player") ||
                other.GetComponentInParent<PlayerReferences>() != null);
    }

    private void OnDisable()
    {
        StopHeartBeep();
    }

    private void OnDestroy()
    {
        StopHeartBeep();
    }

    private void StopHeartBeep()
    {
        playerColliders.Clear();

        if (!heartInstance.isValid())
        {
            heartInstance = default;
            return;
        }

        heartInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        heartInstance.release();
        heartInstance.clearHandle();
        heartInstance = default;
    }
}

using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class HeartBeepZone : MonoBehaviour
{
    [Header("FMOD - Batimento")]
    [SerializeField] private EventReference heartBeep;

    private EventInstance heartInstance;
    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ENTROU ALGO NA ÁREA: " + other.gameObject.name);

        if (!other.CompareTag("Player"))
            return;

        if (playerInside)
            return;

        Debug.Log("PLAYER ENTROU! TOCANDO BATIMENTO.");

        playerInside = true;

        if (heartBeep.IsNull)
        {
            Debug.LogError("ERRO: Heart Beep não foi colocado no Inspector!");
            return;
        }

        heartInstance = RuntimeManager.CreateInstance(heartBeep);

        FMOD.RESULT resultado = heartInstance.start();

        Debug.Log("FMOD START RESULT: " + resultado);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("PLAYER SAIU DA ÁREA.");

        playerInside = false;

        heartInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        heartInstance.release();

        Debug.Log("BATIMENTO PARADO.");
    }
}
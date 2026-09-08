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
        Debug.Log("ALGO ENTROU NA ÁREA: " + other.gameObject.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Não é o Player. Tag: " + other.tag);
            return;
        }

        if (playerInside)
            return;

        Debug.Log("PLAYER ENTROU NA ÁREA DA CAMA!");

        playerInside = true;

        if (heartBeep.IsNull)
        {
            Debug.LogError("O evento FMOD Heart Beep não foi colocado no Inspector!");
            return;
        }

        heartInstance = RuntimeManager.CreateInstance(heartBeep);
        heartInstance.start();

        Debug.Log("BIP DO CORAÇÃO INICIADO!");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("ALGO SAIU DA ÁREA: " + other.gameObject.name);

        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        heartInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        heartInstance.release();

        Debug.Log("BIP DO CORAÇÃO PARADO!");
    }
}
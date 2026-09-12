using UnityEngine;
using FMODUnity;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject Player { get; private set; }

    [Header("FMOD - Som")]
    [SerializeField] private EventReference somCena;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            TocarSomCena();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /* public void RegisterPlayer(GameObject player)
    {
        Player = player;
    }

    public bool HasPlayer()
    {
         return Player != null;
     }*/

    private void TocarSomCena()
    {
        if (!somCena.IsNull)
        {
            RuntimeManager.PlayOneShot(somCena);
        }
    }
}
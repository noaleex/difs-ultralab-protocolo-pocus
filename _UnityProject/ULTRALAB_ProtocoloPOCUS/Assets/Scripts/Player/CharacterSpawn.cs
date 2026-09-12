using UnityEngine;
using Unity.Cinemachine;

public class CharacterSpawn : MonoBehaviour
{
    [SerializeField] private GameObject prefabMasculino;
    [SerializeField] private GameObject prefabFeminino;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    /*
    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (GameManager.Instance.HasPlayer())
        {
            if (cinemachineCamera != null)
            {
                cinemachineCamera.Follow = GameManager.Instance.Player.transform;
            }
            return;
        }

        string characterEscolhido = PlayerPrefs.GetString("Character", "");

        GameObject prefabParaInstanciar = null;

        if (characterEscolhido == "Masculino")
        {
            prefabParaInstanciar = prefabMasculino;
        }
        else if (characterEscolhido == "Feminino" || characterEscolhido == "")
        {
            prefabParaInstanciar = prefabFeminino;
        }

        GameObject novoPersonagem = Instantiate(
            prefabParaInstanciar,
            spawnPoint.position,
            spawnPoint.rotation
        );

        DontDestroyOnLoad(novoPersonagem);

        PlayerReferences references = novoPersonagem.GetComponent<PlayerReferences>();
        if (references != null)
        {
            references.RefreshReferences();
            if (AndroidControl.Instance != null)
            {
                AndroidControl.Instance.SetPlayerInteraction(references.InteractionDetector);
            }
        }

        GameManager.Instance.RegisterPlayer(novoPersonagem);

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = novoPersonagem.transform;
        }
    }
    */
}
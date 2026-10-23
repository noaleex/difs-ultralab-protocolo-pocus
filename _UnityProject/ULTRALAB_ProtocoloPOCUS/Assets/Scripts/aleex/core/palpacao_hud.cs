using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class palpacao_hud : MonoBehaviour
{
    [Header("Elementos da Interface")]
    public Image pacienteSprite;
    public TMP_Text pacienteNome;
    public TMP_Text resumoCaso;
    public TMP_Text dadosPalpacaoInfo; // Onde vai aparecer o texto do achado

    // Método chamado pelo script de interação do paciente
    public void AbrirPalpacao(PatientData dados)
    {
        pacienteSprite.sprite = dados.characterFullSprite;
        pacienteNome.text = dados.patientName;
        resumoCaso.text = dados.resumeCaso; // CORRIGIDO: Era resumoCaso, não pacienteCaso

        // CORRIGIDO: Listas usam .Count em vez de .Length
        if (dados.physicalExam.Count > 0)
        {
            dadosPalpacaoInfo.text = dados.physicalExam[0].info; 
        }

        gameObject.SetActive(true);
    }
}
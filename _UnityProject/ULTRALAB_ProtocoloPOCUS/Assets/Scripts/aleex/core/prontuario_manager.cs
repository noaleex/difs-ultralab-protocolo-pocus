using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class prontuario_manager : MonoBehaviour
{
    public static prontuario_manager instance { get; private set; }

    [Header("Gabarito (Pacientes do Dia)")]
    public List<PatientData> pacientesDoDia; 

    [Header("Campos da UI do Prontuário")]
    public TMP_InputField inputFrequenciaCardiaca;
    public TMP_InputField inputSaturacao;
    public TMP_Dropdown dropdownRitmoCardiaco;
    // Adicione os outros campos aqui...

    [Header("Feedback Visual (Opcional)")]
    public TMP_Text textoNotaTela;

    private PatientData pacienteSelecionado;
    public string notaFinal { get; private set; } = "N/A";

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        // Ao abrir o prontuário, carrega o paciente atual (ex: o primeiro da lista)
        if (pacientesDoDia.Count > 0)
        {
            pacienteSelecionado = pacientesDoDia[0];
        }
        notaFinal = "N/A"; // Reseta a nota ao abrir
    }

    // Botão "VALIDAR" da sua UI chama esta função
    public void ValidarProntuario()
    {
        if (pacienteSelecionado == null) return;

        int pontos = 0;
        int maxPontos = 30; // Ajuste conforme a quantidade de campos valendo pontos (ex: 3 campos x 10 pts)

        // 1. Validando InputField (Ex: Frequência Cardíaca)
        if (inputFrequenciaCardiaca.text == pacienteSelecionado.frequenciaCardiacaGabarito) // Crie essa string no seu PatientData
            pontos += 10;
        else
            pontos -= 5;

        // 2. Validando InputField (Ex: Saturação)
        if (inputSaturacao.text == pacienteSelecionado.saturacaoGabarito)
            pontos += 10;
        else
            pontos -= 5;

        // 3. Validando Dropdown (Ex: Ritmo Cardíaco)
        // Pega o texto da opção selecionada no dropdown e compara
        string ritmoSelecionado = dropdownRitmoCardiaco.options[dropdownRitmoCardiaco.value].text;
        if (ritmoSelecionado == pacienteSelecionado.ritmoCardiacoGabarito)
            pontos += 10;
        else
            pontos -= 5;

        // --- CÁLCULO DA NOTA ---
        // Evita pontuação negativa no total
        pontos = Mathf.Max(0, pontos); 
        float porcentagem = (float)pontos / maxPontos;

        if (porcentagem >= 0.8f) notaFinal = "A";
        else if (porcentagem >= 0.6f) notaFinal = "B";
        else if (porcentagem >= 0.4f) notaFinal = "C";
        else notaFinal = "D"; // Insuficiente

        if (textoNotaTela != null)
        {
            textoNotaTela.text = $"Nota do Plantão: {notaFinal}";
        }

        Debug.Log($"[SISTEMA] Prontuário Validado. Pontos: {pontos}/{maxPontos}. Nota: {notaFinal}");
    }
}
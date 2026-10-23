using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Patient", menuName = "Patients/Patient Data")]
public class PatientData : ScriptableObject
{
    [Header("Geral")]
    public string patientName;
    public int age;
    public string sex;
    public bool tutorial;
    public int welfareScore = 50;

    [Header("Visual")]
    public Sprite characterSprite;
    public Sprite characterFullSprite;
    public Sprite backgroundSprite;

    [Header("Caso")]
    [TextArea]
    public string resumeCaso;
    
    [Header("Exame Físico")]
    public List<PhysicalExamInfo> physicalExam;

    [Header("Via Aérea")]
    public string permeabilidade;
    public string presenca;
    public string intervencao;

    [Header("Respiração")]
    public string frequencia;
    public string saturation;
    public string acessoria;
    public string padraoRespiratorio;
    public string asculta;
    public string expansibilidade;
    public string oxigenoterapiaTipo;
    public string oxigenoterapiaFluxo;

    [Header("Circulação")]
    public string frequenciaCardiaca;
    public string pressaoArterial;
    public string pressaoArterial2;
    public string perfusaoTempo;
    public string perfusaoExtremidades;
    public string pulsos;
    public string ritmoCardiaco;
    public string edema;
    public string temperatura;

    [Header("Avaliação neurológica")]
    public string nivelConsciencia;

    [Header("Exposição")]
    public string avalicaoPele;
    public string presencaPele;
    public string dorEscala;

    [Header("GABARITO - VIA AÉREA")]
    public string permeabilidadeGabarito = "Pérvia";
    public string edemaViaAereaGabarito = "Não";
    public string intervencaoGabarito = "Não";

    [Header("GABARITO - RESPIRAÇÃO")]
    public string frequenciaRespiratoriaGabarito = "24";
    public string saturacaoGabarito = "93";
    public string acessoriaGabarito = "Sim";
    public string padraoRespiratorioGabarito = "Dispneico";
    public string auscultaGabarito = "Estertores";
    public string expansibilidadeGabarito = "Simétrica";
    public string oxigenoterapiaTipoGabarito = "Não";
    public string oxigenoterapiaFluxoGabarito = "0";

    [Header("GABARITO - CIRCULAÇÃO")]
    public string frequenciaCardiacaGabarito = "96";
    public string pressaoSistolicaGabarito = "104";
    public string pressaoDiastolicaGabarito = "68";
    public string temperaturaGabarito = "36,5";
    public string pulsosGabarito = "Palpáveis";
    public string edemaCirculacaoGabarito = "Sim";
    public string ritmoCardiacoGabarito = "Regular";
    public string perfusaoTempoGabarito = "Normal";
    public string perfusaoExtremidadesGabarito = "Frias";

    [Header("GABARITO - NEUROLÓGICA E EXPOSIÇÃO")]
    public string nivelConscienciaGabarito = "Alerta";
    public string dorEscalaGabarito = "5";
    public string avaliacaoPeleGabarito = "Hipocorado";
    public string presencaPeleGabarito = "Edema";
}

[System.Serializable]
public class PhysicalExamInfo
{
    public ButtonBodyArea.BodyRegion region;

    [TextArea]
    public string info;
}
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
}

[System.Serializable]
public class PhysicalExamInfo
{
    public ButtonBodyArea.BodyRegion region;

    [TextArea]
    public string info;
}
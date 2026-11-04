using System;

[Serializable]
public class ConductState
{
    // VIA AÉREA
    public int permeabilidade;
    public int presenca;
    public int intervencao;

    // RESPIRAÇÃO
    public string frequencia;
    public string saturation;
    public int acessoria;
    public int padraoRespiratorio;
    public int asculta;
    public int expansibilidade;
    public int oxigenoterapiaTipo;
    public string oxigenoterapiaFluxo;

    // CIRCULAÇÃO
    public string frequenciaCardiaca;
    public string pressaoArterial;
    public string pressaoArterial2;
    public string perfusaoTempo;
    public int perfusaoExtremidades;
    public int pulsos;
    public int ritmoCardiaco;
    public int edema;
    public string temperatura;

    // DIAGNÓSTICO
    public int diagnostico;

    // NEUROLÓGICO
    public int nivelConsciencia;

    // EXPOSIÇÃO
    public int avalicaoPele;
    public int presencaPele;
    public string dorEscala;

    public void Clear()
    {
        // VIA AÉREA
        permeabilidade = 0;
        presenca = 0;
        intervencao = 0;

        // RESPIRAÇÃO
        frequencia = "";
        saturation = "";

        acessoria = 0;
        padraoRespiratorio = 0;
        asculta = 0;
        expansibilidade = 0;
        oxigenoterapiaTipo = 0;

        oxigenoterapiaFluxo = "";

        // CIRCULAÇÃO
        frequenciaCardiaca = "";
        pressaoArterial = "";
        pressaoArterial2 = "";
        perfusaoTempo = "";

        perfusaoExtremidades = 0;
        pulsos = 0;
        ritmoCardiaco = 0;
        edema = 0;

        temperatura = "";

        // DIAGNÓSTICO
        diagnostico = 0;

        // NEUROLÓGICO
        nivelConsciencia = 0;

        // EXPOSIÇÃO
        avalicaoPele = 0;
        presencaPele = 0;

        dorEscala = "";
    }
}
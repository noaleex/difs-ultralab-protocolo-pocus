using System.Collections.Generic;
using UnityEngine;

public static class GameSession
{
    // =========================================================
    // TEMPO
    // =========================================================

    public static bool HasSavedTime { get; private set; }

    public static int CurrentDay { get; private set; } = 1;
    public static int CurrentHour { get; private set; } = 8;
    public static int CurrentMinute { get; private set; } = 0;

    public static string OriginalSceneName { get; private set; }


    // =========================================================
    // ESTADO DOS PACIENTES
    // =========================================================

    private class PatientState
    {
        public int welfareScore;
        public bool discharged;

        public ConductState conductState;
        public bool hasConduct;
        public bool conductCompleted;
    }

    private static readonly Dictionary<PatientData, PatientState>
        patientStates =
            new Dictionary<PatientData, PatientState>();


    // =========================================================
    // TEMPO
    // =========================================================

    public static void SaveTime(
    int day,
    int hour,
    int minute)
{
    CurrentDay = day;
    CurrentHour = hour;
    CurrentMinute = minute;

    HasSavedTime = true;
}

    public static void LoadTime(
        out int day,
        out int hour,
        out int minute)
    {
        day = CurrentDay;
        hour = CurrentHour;
        minute = CurrentMinute;
    }


    // =========================================================
    // CENA ORIGINAL
    // =========================================================

    public static void SetOriginalScene(
        string sceneName)
    {
        OriginalSceneName = sceneName;

    }


    // =========================================================
    // ESTADO DO PACIENTE
    // =========================================================

    private static PatientState GetOrCreatePatientState(
        PatientData patient)
    {
        if (patient == null)
            return null;

        if (!patientStates.TryGetValue(
            patient,
            out PatientState state))
        {
            state = new PatientState();

            state.welfareScore =
                patient.welfareScore;

            state.discharged = false;

            state.conductState =
                new ConductState();

            state.conductState.Clear();

            state.hasConduct = false;
            state.conductCompleted = false;

            patientStates.Add(
                patient,
                state
            );

        }

        return state;
    }


    // =========================================================
    // WELFARE - OBTER VALOR ATUAL
    // =========================================================

    public static int GetPatientWelfare(
        PatientData patient)
    {
        if (patient == null)
        {
            Debug.LogError(
                "[GameSession] PatientData nulo ao tentar " +
                "obter welfare."
            );

            return 0;
        }

        PatientState state =
            GetOrCreatePatientState(patient);

        return state.welfareScore;
    }


    // =========================================================
    // WELFARE - ALTERAR VALOR ATUAL
    // =========================================================

    public static void SetPatientWelfare(
        PatientData patient,
        int value)
    {
        if (patient == null)
        {
            Debug.LogError(
                "[GameSession] PatientData nulo ao tentar " +
                "alterar welfare."
            );

            return;
        }

        PatientState state =
            GetOrCreatePatientState(patient);

        state.welfareScore =
            Mathf.Clamp(
                value,
                0,
                74
            );

    }


    // =========================================================
    // SALVAR PACIENTE
    // =========================================================

    public static void SavePatient(
        PatientData patient)
    {
        if (patient == null)
            return;

        PatientState state =
            GetOrCreatePatientState(patient);
    }


    // =========================================================
    // CARREGAR PACIENTE
    // =========================================================

    public static void LoadPatient(
        PatientData patient)
    {
        if (patient == null)
            return;

        PatientState state =
            GetOrCreatePatientState(patient);

    }


    // =========================================================
    // ALTA
    // =========================================================

    public static void MarkPatientAsDischarged(
        PatientData patient)
    {
        if (patient == null)
            return;

        PatientState state =
            GetOrCreatePatientState(patient);

        state.discharged = true;

        Debug.Log(
            $"[GameSession] " +
            $"{patient.patientName} recebeu alta."
        );
    }

    public static bool IsPatientDischarged(
        PatientData patient)
    {
        if (patient == null)
            return false;

        if (patientStates.TryGetValue(
            patient,
            out PatientState state))
        {
            return state.discharged;
        }

        return false;
    }


    // =========================================================
    // CONDUTA
    // =========================================================

    public static void SaveConduct(
        PatientData patient,
        ConductState conductState,
        bool conductCompleted)
    {
        if (patient == null ||
            conductState == null)
        {
            return;
        }

        PatientState state =
            GetOrCreatePatientState(patient);

        state.conductState =
            CopyConductState(
                conductState
            );

        state.hasConduct = true;

        state.conductCompleted =
            conductCompleted;

        Debug.Log(
            $"[GameSession] Conduta salva: " +
            $"{patient.patientName}"
        );
    }


    // =========================================================
    // CARREGAR CONDUTA
    // =========================================================

    public static bool LoadConduct(
        PatientData patient,
        ConductState targetState,
        out bool conductCompleted)
    {
        conductCompleted = false;

        if (patient == null ||
            targetState == null)
        {
            return false;
        }

        if (!patientStates.TryGetValue(
            patient,
            out PatientState state))
        {
            return false;
        }

        if (!state.hasConduct ||
            state.conductState == null)
        {
            return false;
        }

        CopyConductState(
            state.conductState,
            targetState
        );

        conductCompleted =
            state.conductCompleted;

        Debug.Log(
            $"[GameSession] Conduta restaurada: " +
            $"{patient.patientName}"
        );

        return true;
    }


    // =========================================================
    // COPIAR CONDUCT STATE
    // =========================================================

    private static ConductState CopyConductState(
        ConductState source)
    {
        if (source == null)
            return null;

        ConductState copy =
            new ConductState();

        CopyConductState(
            source,
            copy
        );

        return copy;
    }


    private static void CopyConductState(
        ConductState source,
        ConductState target)
    {
        if (source == null ||
            target == null)
        {
            return;
        }

        // =====================================================
        // VIA AÉREA
        // =====================================================

        target.permeabilidade =
            source.permeabilidade;

        target.presenca =
            source.presenca;

        target.intervencao =
            source.intervencao;


        // =====================================================
        // RESPIRAÇÃO
        // =====================================================

        target.frequencia =
            source.frequencia;

        target.saturation =
            source.saturation;

        target.acessoria =
            source.acessoria;

        target.padraoRespiratorio =
            source.padraoRespiratorio;

        target.asculta =
            source.asculta;

        target.expansibilidade =
            source.expansibilidade;

        target.oxigenoterapiaTipo =
            source.oxigenoterapiaTipo;

        target.oxigenoterapiaFluxo =
            source.oxigenoterapiaFluxo;


        // =====================================================
        // CIRCULAÇÃO
        // =====================================================

        target.frequenciaCardiaca =
            source.frequenciaCardiaca;

        target.pressaoArterial =
            source.pressaoArterial;

        target.pressaoArterial2 =
            source.pressaoArterial2;

        target.perfusaoTempo =
            source.perfusaoTempo;

        target.perfusaoExtremidades =
            source.perfusaoExtremidades;

        target.pulsos =
            source.pulsos;

        target.ritmoCardiaco =
            source.ritmoCardiaco;

        target.edema =
            source.edema;

        target.temperatura =
            source.temperatura;


        // =====================================================
        // NEUROLÓGICO
        // =====================================================

        target.nivelConsciencia =
            source.nivelConsciencia;


        // =====================================================
        // EXPOSIÇÃO
        // =====================================================

        target.avalicaoPele =
            source.avalicaoPele;

        target.presencaPele =
            source.presencaPele;

        target.dorEscala =
            source.dorEscala;
    }


    // =========================================================
    // NOVO JOGO
    // =========================================================

    public static void ResetSession(
        int startHour = 8)
    {
        HasSavedTime = false;

        CurrentDay = 1;

        CurrentHour =
            startHour;

        CurrentMinute = 0;

        OriginalSceneName = null;

        patientStates.Clear();

        Debug.Log(
            "[GameSession] Nova partida iniciada."
        );
    }
}
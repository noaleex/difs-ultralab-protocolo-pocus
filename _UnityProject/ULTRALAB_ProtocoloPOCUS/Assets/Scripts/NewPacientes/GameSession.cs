using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameSession
{

    // =========================================================
    // RESULTADOS DIÁRIOS (FICAM PENDENTES ATÉ O NPC SER CONSULTADO)
    // =========================================================

    public sealed class DailyEvaluationResult
    {
        public PatientData Patient { get; private set; }
        public int Day { get; private set; }
        public int Correct { get; private set; }
        public int Total { get; private set; }
        public bool Collected { get; internal set; }

        internal DailyEvaluationResult(PatientData patient, int day, int correct, int total)
        {
            Patient = patient;
            Day = day;
            Total = Mathf.Max(0, total);
            Correct = Mathf.Clamp(correct, 0, Total);
            Collected = false;
        }
    }

    private static readonly List<DailyEvaluationResult> dailyResults =
        new List<DailyEvaluationResult>();

    public static event Action<PatientData, int, int> PatientWelfareChanged;
    public static event Action<int> DayChanged;

    public static void RecordDailyEvaluation(PatientData patient, int day, int correct, int total)
    {
        if (patient == null) return;

        // Uma única avaliação por paciente/dia: uma avaliação posterior substitui a anterior.
        for (int i = 0; i < dailyResults.Count; i++)
        {
            DailyEvaluationResult existing = dailyResults[i];
            if (existing.Patient == patient && existing.Day == day && !existing.Collected)
            {
                dailyResults[i] = new DailyEvaluationResult(patient, day, correct, total);
                return;
            }
        }

        dailyResults.Add(new DailyEvaluationResult(patient, day, correct, total));
    }

    public static List<DailyEvaluationResult> GetPendingDailyResults()
    {
        List<DailyEvaluationResult> pending = new List<DailyEvaluationResult>();
        foreach (DailyEvaluationResult result in dailyResults)
        {
            if (result != null && !result.Collected) pending.Add(result);
        }
        pending.Sort((a, b) =>
        {
            int dayCompare = a.Day.CompareTo(b.Day);
            return dayCompare != 0 ? dayCompare : string.Compare(a.Patient != null ? a.Patient.patientName : string.Empty, b.Patient != null ? b.Patient.patientName : string.Empty, StringComparison.Ordinal);
        });
        return pending;
    }

    public static void MarkDailyResultsCollected(IEnumerable<DailyEvaluationResult> results)
    {
        if (results == null) return;
        foreach (DailyEvaluationResult result in results)
        {
            if (result != null) result.Collected = true;
        }
    }

    public static bool AreAllPatientsDischarged(int requiredCount)
    {
        if (requiredCount <= 0) return true;
        int count = 0;
        foreach (KeyValuePair<PatientData, PatientState> entry in patientStates)
        {
            if (entry.Key != null && entry.Value.discharged) count++;
        }
        return count >= requiredCount;
    }

    private static bool AreConfiguredPatientsDischarged()
    {
        return progressionPatient1 != null && progressionPatient2 != null && progressionPatient3 != null &&
               IsPatientDischarged(progressionPatient1) &&
               IsPatientDischarged(progressionPatient2) &&
               IsPatientDischarged(progressionPatient3);
    }

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

    // Progressão dos três pacientes. Esses dados ficam na sessão ao trocar de cena.
    private static PatientData progressionPatient1;
    private static PatientData progressionPatient2;
    private static PatientData progressionPatient3;
    // Unlocks are scheduled for the next in-game day, never during the current day.
    private static int patient2UnlockDay = -1;
    private static int patient3UnlockDay = -1;
    private static int progressionSpawnThreshold = 50;

    public static bool IsPatient2Unlocked => patient2UnlockDay > 0 && CurrentDay >= patient2UnlockDay;
    public static bool IsPatient3Unlocked => patient3UnlockDay > 0 && CurrentDay >= patient3UnlockDay;

    public static void ConfigurePatientProgression(
        PatientData patient1, PatientData patient2, PatientData patient3, int spawnThreshold = 50)
    {
        if (patient1 == null || patient2 == null || patient3 == null)
        {
            Debug.LogError("[GameSession] ConfigurePatientProgression recebeu PatientData nulo.");
            return;
        }

        bool changedConfiguration = progressionPatient1 != patient1 ||
                                   progressionPatient2 != patient2 ||
                                   progressionPatient3 != patient3;

        progressionPatient1 = patient1;
        progressionPatient2 = patient2;
        progressionPatient3 = patient3;
        progressionSpawnThreshold = Mathf.Max(0, spawnThreshold);

        if (changedConfiguration)
        {
            patient2UnlockDay = -1;
            patient3UnlockDay = -1;
        }
    }


    // =========================================================
    // TEMPO
    // =========================================================

    public static void SaveTime(
    int day,
    int hour,
    int minute)
{
    int previousDay = CurrentDay;
    CurrentDay = day;
    CurrentHour = hour;
    CurrentMinute = minute;

    HasSavedTime = true;

    if (CurrentDay != previousDay)
        DayChanged?.Invoke(CurrentDay);
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

        int previousValue = state.welfareScore;
        state.welfareScore = Mathf.Clamp(value, 0, 74);
        int currentValue = state.welfareScore;

        if (patient == progressionPatient1 && patient2UnlockDay < 0 &&
            (currentValue >= progressionSpawnThreshold || currentValue >= 74))
        {
            patient2UnlockDay = CurrentDay + 1;
            Debug.Log($"[GameSession] Paciente 2 liberado para o dia {patient2UnlockDay}; não aparecerá hoje.");
        }

        if (currentValue >= 74)
        {
            PatientState currentState = GetOrCreatePatientState(patient);
            currentState.discharged = true;
            if (patient == progressionPatient2 && patient3UnlockDay < 0)
            {
                patient3UnlockDay = CurrentDay + 1;
                Debug.Log($"[GameSession] Paciente 3 liberado para o dia {patient3UnlockDay}; não aparecerá hoje.");
            }
        }

        if (previousValue != currentValue)
            PatientWelfareChanged?.Invoke(patient, previousValue, currentValue);

        if (currentValue <= 0)
            GameEndingManager.FinishGame(GameEndingManager.EndingType.Negative);
        else if (AreConfiguredPatientsDischarged())
            GameEndingManager.FinishGame(GameEndingManager.EndingType.Positive);
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
        if (patient == progressionPatient1 && patient2UnlockDay < 0)
            patient2UnlockDay = CurrentDay + 1;
        if (patient == progressionPatient2 && patient3UnlockDay < 0)
            patient3UnlockDay = CurrentDay + 1;

        Debug.Log(
            $"[GameSession] " +
            $"{patient.patientName} recebeu alta."
        );

        if (AreConfiguredPatientsDischarged())
            GameEndingManager.FinishGame(GameEndingManager.EndingType.Positive);
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
        dailyResults.Clear();
        progressionPatient1 = null;
        progressionPatient2 = null;
        progressionPatient3 = null;
        patient2UnlockDay = -1;
        patient3UnlockDay = -1;
        progressionSpawnThreshold = 50;
        GameEndingManager.ResetEndingState();

        Debug.Log(
            "[GameSession] Nova partida iniciada."
        );
    }
}
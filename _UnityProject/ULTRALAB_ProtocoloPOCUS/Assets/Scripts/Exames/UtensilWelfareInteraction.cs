using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Interações do cabide de soro e da mesinha, vinculadas ao paciente selecionado.</summary>
public sealed class UtensilWelfareInteraction : MonoBehaviour, IInteractable
{
    public enum UtensilType
    {
        CabideDeSoro,
        Mesinha
    }

    private sealed class PatientUseState
    {
        public int day;
        public bool tableUsed;
        public bool hasIvStandUse;
        public int lastIvStandMinute;
    }

    private static readonly Dictionary<int, PatientUseState> States = new Dictionary<int, PatientUseState>();

    [Header("Configuração")]
    [SerializeField] private UtensilType utensilType;
    [SerializeField, Min(1)] private int welfareReward = 2;
    [SerializeField, Min(1)] private int welfarePenalty = 1;
    [SerializeField, Min(1)] private int minutesBetweenIvStandRewards = 60;

    [Header("Feedback opcional")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField, Min(0.1f)] private float feedbackDuration = 2.5f;

    private Coroutine feedbackCoroutine;

    public bool CanInteract() => CurrentPatient.Data != null;

    public void Interact()
    {
        PatientData patient = CurrentPatient.Data;
        if (patient == null)
        {
            ShowFeedback("Selecione um paciente antes de ajustar o utensílio.");
            return;
        }

        int day = Timer.Instance != null ? Mathf.Max(1, Timer.Instance.CurrentDay) : 1;
        int currentMinute = GetMinutesOfDay();
        PatientUseState state = GetState(patient, day);

        if (utensilType == UtensilType.CabideDeSoro)
            UseIvStand(patient, state, currentMinute);
        else
            UseTable(patient, state);
    }

    private void UseIvStand(PatientData patient, PatientUseState state, int currentMinute)
    {
        if (!state.hasIvStandUse)
        {
            state.hasIvStandUse = true;
            state.lastIvStandMinute = currentMinute;
            ChangeWelfare(patient, welfareReward);
            ShowFeedback("Item ajustado");
            return;
        }

        int elapsed = currentMinute - state.lastIvStandMinute;
        if (elapsed >= minutesBetweenIvStandRewards)
        {
            state.lastIvStandMinute = currentMinute;
            ChangeWelfare(patient, welfareReward);
            ShowFeedback("Item ajustado");
            return;
        }

        ChangeWelfare(patient, -welfarePenalty);
        ShowFeedback("Você está perdendo tempo pois o utensílio já foi arrumado.");
    }

    private void UseTable(PatientData patient, PatientUseState state)
    {
        if (!state.tableUsed)
        {
            state.tableUsed = true;
            ChangeWelfare(patient, 10);
            ShowFeedback("Item ajustado");
            return;
        }

        ChangeWelfare(patient, -welfarePenalty);
        ShowFeedback("Você está perdendo tempo pois o utensílio já foi arrumado.");
    }

    private static PatientUseState GetState(PatientData patient, int day)
    {
        int id = patient.GetInstanceID();
        if (!States.TryGetValue(id, out PatientUseState state) || state.day != day)
        {
            state = new PatientUseState { day = day };
            States[id] = state;
        }
        return state;
    }

    private static int GetMinutesOfDay()
    {
        if (Timer.Instance == null) return 8 * 60;
        return Timer.Instance.CurrentHour * 60 + Timer.Instance.CurrentMinute;
    }

    private static void ChangeWelfare(PatientData patient, int delta)
    {
        int before = GameSession.GetPatientWelfare(patient);
        int after = Mathf.Clamp(before + delta, 0, 74);
        GameSession.SetPatientWelfare(patient, after);
        GameSession.SavePatient(patient);
        Debug.Log($"[Utensílio] Paciente: {patient.patientName} | Welfare: {before} -> {after} ({(delta >= 0 ? "+" : string.Empty)}{delta})");
    }

    private void ShowFeedback(string message)
    {
        Debug.Log(message, this);
        if (feedbackText == null) return;
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        feedbackCoroutine = StartCoroutine(HideFeedback());
    }

    private System.Collections.IEnumerator HideFeedback()
    {
        yield return new WaitForSecondsRealtime(feedbackDuration);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        feedbackCoroutine = null;
    }

    private void OnDisable()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }
    }
}

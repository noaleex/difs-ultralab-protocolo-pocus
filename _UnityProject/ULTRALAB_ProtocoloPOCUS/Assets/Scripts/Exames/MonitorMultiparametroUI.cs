using System.Text;
using TMPro;
using UnityEngine;

/// <summary>Exibe no monitor os dados clínicos atuais do paciente selecionado.</summary>
public sealed class MonitorMultiparametroUI : MonoBehaviour
{
    [Header("Monitor")]
    [SerializeField] private TMP_Text monitorText;
    [SerializeField] private TMP_Text monitorText2;
    [SerializeField] private string mensagemSemPaciente = "Nenhum paciente selecionado";

    private int lastDay = -1;
    private int lastHour = -1;
    private int lastMinute = -1;
    private PatientData lastPatient;

    private void OnEnable()
    {
        CurrentPatient.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        CurrentPatient.Changed -= Refresh;
    }

    private void Update()
    {
        // Atualiza quando o relógio muda, sem reconstruir o texto a cada frame.
        int day = Timer.Instance != null ? Timer.Instance.CurrentDay : 1;
        int hour = Timer.Instance != null ? Timer.Instance.CurrentHour : -1;
        int minute = Timer.Instance != null ? Timer.Instance.CurrentMinute : -1;
        if (day != lastDay || hour != lastHour || minute != lastMinute || lastPatient != CurrentPatient.Data)
            Refresh();
    }

    

public void Refresh()
{
    if (monitorText == null || monitorText2 == null)
        return;

    PatientData patient = CurrentPatient.Data;

    if (patient == null)
    {
        monitorText.text = mensagemSemPaciente;
        monitorText2.text = mensagemSemPaciente;

        lastPatient = null;
        StoreClock();
        return;
    }

    int day = Timer.Instance != null
        ? Mathf.Max(1, Timer.Instance.CurrentDay)
        : 1;

    PatientData data = PatientDailyState.GetClinicalData(patient, day);

    if (data == null)
    {
        monitorText.text = "Não foi possível carregar os dados clínicos.";
        monitorText2.text = "Não foi possível carregar os dados clínicos.";

        StoreClock();
        return;
    }

    string horario = Timer.Instance != null
        ? $"{Timer.Instance.CurrentHour:00}:{Timer.Instance.CurrentMinute:00}"
        : "--:--";

    // MONITOR 1 — CARDÍACO E PRESSÃO ARTERIAL
    StringBuilder sb1 = new StringBuilder(256);

    sb1.AppendLine("MONITOR MULTIPARÂMETRO");
    sb1.AppendLine();

    sb1.AppendLine("CARDÍACO");
    AddLine(sb1, "Frequência cardíaca", data.frequenciaCardiaca, " bpm");
    AddLine(sb1, "Ritmo cardíaco", data.ritmoCardiaco);

    sb1.AppendLine();
    sb1.AppendLine("PRESSÃO ARTERIAL");

    AddLine(
        sb1,
        "Pressão arterial",
        Join(data.pressaoArterial, data.pressaoArterial2),
        " mmHg"
    );

    // MONITOR 2 — RESPIRATÓRIO E TEMPERATURA
    StringBuilder sb2 = new StringBuilder(256);

    sb2.AppendLine("MONITORIZAÇÃO RESPIRATÓRIA");
    sb2.AppendLine($"Paciente: {Value(data.patientName)}");
    sb2.AppendLine();

    sb2.AppendLine("RESPIRATÓRIO");
    AddLine(sb2, "Saturação de oxigênio (SpO)", data.saturation, "%");
    AddLine(sb2, "Frequência respiratória", data.frequencia, " irpm");

    if (!string.IsNullOrWhiteSpace(data.temperatura))
    {
        sb2.AppendLine();
        sb2.AppendLine("TEMPERATURA");
        AddLine(sb2, "Temperatura", data.temperatura, " °C");
    }

    monitorText.text = sb1.ToString();
    monitorText2.text = sb2.ToString();

    lastPatient = patient;
    StoreClock();
}



    private void StoreClock()
    {
        lastDay = Timer.Instance != null ? Timer.Instance.CurrentDay : 1;
        lastHour = Timer.Instance != null ? Timer.Instance.CurrentHour : -1;
        lastMinute = Timer.Instance != null ? Timer.Instance.CurrentMinute : -1;
    }

    private static void AddLine(StringBuilder sb, string label, string value, string suffix = "")
    {
        if (!string.IsNullOrWhiteSpace(value))
            sb.AppendLine($"{label}: {value}{suffix}");
    }

    private static string Value(string value) => string.IsNullOrWhiteSpace(value) ? "Não informado" : value;

    private static string Join(string first, string second)
    {
        if (string.IsNullOrWhiteSpace(first)) return second;
        if (string.IsNullOrWhiteSpace(second)) return first;
        return first + " / " + second;
    }
}

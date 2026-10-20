using System.Collections.Generic;
using UnityEngine;

public class PatientManager : MonoBehaviour
{
    public static PatientManager Instance;

    private readonly List<OpenExams> patients = new List<OpenExams>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PatientManager] Instância duplicada; removendo o objeto duplicado.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegisterPatient(OpenExams patient)
    {
        if (patient == null)
            return;

        RemoveDestroyedPatients();

        if (patients.Contains(patient))
            return;

        patients.Add(patient);
        Debug.Log($"[PatientManager] Registrado: {GetPatientName(patient)}. Total ativo: {patients.Count}.", patient);
    }

    public void UnregisterPatient(OpenExams patient)
    {
        if (ReferenceEquals(patient, null))
            return;

        patients.Remove(patient);
        RemoveDestroyedPatients();
    }

    /// <summary>
    /// Retorna uma cópia estável para que alterações na lista durante uma avaliação
    /// não invalidem a iteração.
    /// </summary>
    public IReadOnlyList<OpenExams> GetPatients()
    {
        RemoveDestroyedPatients();
        return patients.ToArray();
    }

    public bool AllPatientsCompletedConduct()
    {
        RemoveDestroyedPatients();

        if (patients.Count == 0)
        {
            Debug.LogError("[PatientManager] Nenhum paciente foi registrado. O SkipDay será bloqueado para evitar avançar com uma lista vazia.", this);
            return false;
        }

        bool allCompleted = true;

        foreach (OpenExams patient in patients)
        {
            if (patient == null)
                continue;

            if (patient.PatientDataReference == null)
            {
                Debug.LogError("[PatientManager] Existe um paciente registrado sem PatientData. O SkipDay será bloqueado.", patient);
                allCompleted = false;
                continue;
            }

            if (!patient.ConductCompleted)
            {
                Debug.LogWarning($"[PatientManager] Conduta pendente: {GetPatientName(patient)}.", patient);
                allCompleted = false;
            }
            else
            {
                Debug.Log($"[PatientManager] Conduta concluída: {GetPatientName(patient)}.", patient);
            }
        }

        return allCompleted;
    }

    private void RemoveDestroyedPatients()
    {
        for (int i = patients.Count - 1; i >= 0; i--)
        {
            if (patients[i] == null)
                patients.RemoveAt(i);
        }
    }

    private static string GetPatientName(OpenExams patient)
    {
        if (patient == null || patient.PatientDataReference == null)
            return "Paciente sem identificação";

        return patient.PatientDataReference.patientName;
    }
}

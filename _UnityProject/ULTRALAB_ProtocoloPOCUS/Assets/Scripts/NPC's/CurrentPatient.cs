/// <summary>
/// Guarda a seleção atual do paciente sem procurar objetos na cena.
/// A seleção é definida diretamente pelo OpenExams que recebeu a interação.
/// </summary>
public static class CurrentPatient
{
    public static PatientData Data;
    public static OpenExams Object;

    public static void Select(OpenExams patient)
    {
        if (patient == null)
        {
            Clear();
            return;
        }

        Object = patient;
        Data = patient.PatientDataReference;
    }

    public static void Clear()
    {
        Data = null;
        Object = null;
    }

    public static void ClearIfCurrent(OpenExams patient)
    {
        if (Object == patient)
            Clear();
    }
}

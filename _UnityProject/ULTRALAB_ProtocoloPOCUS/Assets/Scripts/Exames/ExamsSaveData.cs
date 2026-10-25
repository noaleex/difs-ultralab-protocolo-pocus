using UnityEngine;

public static class ExamsSaveData
{
    public static Sprite SavedImage;
    public static BodyArea.BodyRegion? SavedExam;
    public static bool IsDefaultUltrasoundImage;
    public static PatientData OwnerPatient;
    public static int SavedDay = -1;

    public static bool HasImage => SavedImage != null;

    public static void Save(Sprite image, BodyArea.BodyRegion exam, PatientData patient, int day)
    {
        SavedImage = image;
        SavedExam = exam;
        OwnerPatient = patient;
        SavedDay = day;
    }

    public static void Clear()
    {
        SavedImage = null;
        SavedExam = null;
        IsDefaultUltrasoundImage = false;
        OwnerPatient = null;
        SavedDay = -1;
    }
}

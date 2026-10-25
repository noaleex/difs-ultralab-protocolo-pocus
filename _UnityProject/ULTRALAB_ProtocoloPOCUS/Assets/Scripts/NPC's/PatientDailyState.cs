using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Gera e mantém uma cópia runtime dos dados clínicos de cada paciente para cada dia.
/// Não altera o PatientData asset original.
/// </summary>
public static class PatientDailyState
{
    private const float ChanceSemUltrassom = 0.25f;

    private sealed class RuntimeEntry
    {
        public int day;
        public PatientData clinicalData;
        public BodyArea.BodyRegion ultrasoundRegion;
        public Sprite ultrasoundImage;
        public bool foodDelivered;
        
    }


    private static readonly Dictionary<int, RuntimeEntry> Entries = new Dictionary<int, RuntimeEntry>();

    public static PatientData GetClinicalData(PatientData source, int day)
    {
        RuntimeEntry entry = GetEntry(source, day);
        return entry != null ? entry.clinicalData : null;
    }

    public static BodyArea.BodyRegion GetUltrasoundRegion(PatientData source, int day)
    {
        RuntimeEntry entry = GetEntry(source, day);
        return entry != null ? entry.ultrasoundRegion : BodyArea.BodyRegion.Empty;
    }

    public static Sprite GetUltrasoundImage(PatientData source, int day)
    {
        RuntimeEntry entry = GetEntry(source, day);
        return entry != null ? entry.ultrasoundImage : null;
    }

    public static bool HasDeliveredFood(PatientData source, int day)
    {
        RuntimeEntry entry = GetEntry(source, day);
        return entry != null && entry.foodDelivered;
    }

    public static bool TryDeliverFood(PatientData source, int day, ChoiceFood.TipoComida selected, out bool correct, out bool alreadyDelivered)
    {
        RuntimeEntry entry = GetEntry(source, day);
        correct = false;
        alreadyDelivered = false;
        if (entry == null) return false;
        if (entry.foodDelivered)
        {
            alreadyDelivered = true;
            return false;
        }

        entry.foodDelivered = true;
        correct = entry.clinicalData.food == (int)selected;
        return true;
    }

    public static bool IsFoodCorrect(PatientData source, int day, ChoiceFood.TipoComida selected)
    {
        RuntimeEntry entry = GetEntry(source, day);
        return entry != null && entry.clinicalData.food == (int)selected;
    }

    public static void PrepareForDay(PatientData source, int day)
    {
        if (source == null) return;
        int id = source.GetInstanceID();
        Sprite previousImage = null;
        if (Entries.TryGetValue(id, out RuntimeEntry existing))
        {
            previousImage = existing.ultrasoundImage;
            if (existing.clinicalData != null) UnityEngine.Object.Destroy(existing.clinicalData);
            Entries.Remove(id);
        }
        CreateEntry(source, day, previousImage);
    }

    public static void ClearPatient(PatientData source)
    {
        if (source == null) return;
        int id = source.GetInstanceID();
        if (!Entries.TryGetValue(id, out RuntimeEntry entry)) return;
        if (entry.clinicalData != null) UnityEngine.Object.Destroy(entry.clinicalData);
        Entries.Remove(id);
    }

    private static RuntimeEntry GetEntry(PatientData source, int day)
    {
        if (source == null) return null;
        int id = source.GetInstanceID();
        if (Entries.TryGetValue(id, out RuntimeEntry entry) && entry.day == day && entry.clinicalData != null)
            return entry;
        Sprite previousImage = entry != null ? entry.ultrasoundImage : null;
        if (entry != null && entry.clinicalData != null) UnityEngine.Object.Destroy(entry.clinicalData);
        Entries.Remove(id);
        return CreateEntry(source, day, previousImage);
    }

    
private static RuntimeEntry CreateEntry(
    PatientData source,
    int day,
    Sprite previousImage = null)
{
    PatientData clone = UnityEngine.Object.Instantiate(source);
    clone.name = source.name + "_Runtime_Day_" + day;

    RandomizeVitals(clone);
    RandomizeFood(clone);

    RuntimeEntry entry = new RuntimeEntry
    {
        day = day,
        clinicalData = clone,
        ultrasoundRegion = BodyArea.BodyRegion.Empty,
        ultrasoundImage = null
    };

    // Em 25% dos casos, nenhum ultrassom será solicitado.
    if (UnityEngine.Random.value < ChanceSemUltrassom)
    {
        Entries[source.GetInstanceID()] = entry;
        return entry;
    }

    if (source.ultrasoundImageOptions != null)
    {
        List<UltrasoundImageOption> valid =
            source.ultrasoundImageOptions.FindAll(
                x => x != null &&
                     x.image != null &&
                     x.region != BodyArea.BodyRegion.Empty
            );

        if (valid.Count > 1 && previousImage != null)
        {
            List<UltrasoundImageOption> alternatives =
                valid.FindAll(x => x.image != previousImage);

            if (alternatives.Count > 0)
                valid = alternatives;
        }

        if (valid.Count > 0)
        {
            UltrasoundImageOption selected =
                valid[UnityEngine.Random.Range(0, valid.Count)];

            entry.ultrasoundRegion = selected.region;
            entry.ultrasoundImage = selected.image;
        }
    }

    Entries[source.GetInstanceID()] = entry;
    return entry;
}


    private static void RandomizeFood(PatientData data)
    {
        // ChoiceFood enum: Nenhuma=0, Solido=1, Liquido=2, NPT=3.
        data.food = UnityEngine.Random.Range(1, 4);
    }

    private static void RandomizeVitals(PatientData data)
    {
        if (int.TryParse(data.frequenciaCardiaca, NumberStyles.Integer, CultureInfo.InvariantCulture, out int heartRate))
        {
            int delta = UnityEngine.Random.Range(1, 5) * (UnityEngine.Random.value < 0.5f ? -1 : 1);
            data.frequenciaCardiaca = Mathf.Clamp(heartRate + delta, 30, 220).ToString(CultureInfo.InvariantCulture);
        }

        if (TryParseDecimal(data.temperatura, out float temperature))
        {
            float delta = UnityEngine.Random.Range(1, 5) / 10f * (UnityEngine.Random.value < 0.5f ? -1f : 1f);
            float randomized = Mathf.Clamp(temperature + delta, 34f, 42f);
            data.temperatura = randomized.ToString("0.0", CultureInfo.InvariantCulture);
        }
    }

    private static bool TryParseDecimal(string value, out float result)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)
            || float.TryParse(value, NumberStyles.Float, new CultureInfo("pt-BR"), out result);
    }
}

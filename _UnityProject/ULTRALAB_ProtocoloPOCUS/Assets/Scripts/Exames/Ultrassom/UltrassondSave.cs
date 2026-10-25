using System.Collections;
using UnityEngine;
using FMODUnity;

public class UltrassondSave : MonoBehaviour
{
    [SerializeField] private UltrasoundManager ultrasoundManager;
    [SerializeField] private GameObject confirmText;

    [Header("Som FMOD")]
    [SerializeField] private EventReference somClick;

    [SerializeField] private float textTime = 1.5f;
    [SerializeField] private Timer timer;

    private Coroutine confirmCoroutine;

    public void OnSaveImage()
    {
        if (ultrasoundManager == null)
        {
            Debug.LogError("UltrasoundManager não foi atribuído.", this);
            return;
        }

        if (ultrasoundManager.resultImage == null || ultrasoundManager.resultImage.sprite == null)
        {
            Debug.LogWarning("Não há imagem de ultrassom para salvar.", this);
            return;
        }

        PatientData patient = CurrentPatient.Data;
        if (patient == null)
        {
            Debug.LogError("Nenhum paciente está selecionado.", this);
            return;
        }

        int currentDay = timer != null
    ? Mathf.Max(1, timer.CurrentDay)
    : Timer.CurrentDayValue;

        bool isDefault = ultrasoundManager.resultImage.sprite == ultrasoundManager.defaultImage;
        BodyArea.BodyRegion region = isDefault
            ? BodyArea.BodyRegion.Empty
            : ultrasoundManager.CurrentRegion;

        if (!isDefault && region == BodyArea.BodyRegion.Empty)
        {
            Debug.LogWarning("Posicione a sonda sobre uma região anatômica válida antes de salvar.", this);
            return;
        }

        ExamsSaveData.Save(
            ultrasoundManager.resultImage.sprite,
            region,
            patient,
            currentDay
        );
        ExamsSaveData.IsDefaultUltrasoundImage = isDefault;

        if (!somClick.IsNull)
            RuntimeManager.PlayOneShot(somClick);

        if (confirmText != null)
        {
            confirmText.SetActive(true);
            if (confirmCoroutine != null)
                StopCoroutine(confirmCoroutine);
            confirmCoroutine = StartCoroutine(HideConfirmation());
        }

        Debug.Log($"Imagem salva para {patient.patientName}, dia {currentDay}: {region}");
    }

    private IEnumerator HideConfirmation()
    {
        yield return new WaitForSeconds(textTime);
        if (confirmText != null)
            confirmText.SetActive(false);
        confirmCoroutine = null;
    }
}

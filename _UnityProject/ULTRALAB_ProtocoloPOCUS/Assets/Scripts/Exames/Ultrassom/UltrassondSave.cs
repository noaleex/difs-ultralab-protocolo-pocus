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

    private Coroutine confirmCoroutine;

    [SerializeField] private Timer timer;

    public void OnSaveImage()
{
    if (ultrasoundManager == null)
    {
        Debug.LogError("UltrasoundManager não foi atribuído.");
        return;
    }

    if (ultrasoundManager.resultImage == null ||
        ultrasoundManager.resultImage.sprite == null)
    {
        Debug.Log("Nenhuma imagem para salvar.");
        return;
    }

    // Identifica o paciente atual.
    PatientData patient = CurrentPatient.Data;

    if (patient == null)
    {
        Debug.LogError("Nenhum paciente está selecionado.");
        return;
    }

    // Identifica se a imagem exibida é a imagem padrão.
    bool isDefault =
        ultrasoundManager.resultImage.sprite ==
        ultrasoundManager.defaultImage;

    if (timer == null)
{
    Debug.LogError("Timer não foi atribuído no Inspector.");
    return;
}

int currentDay = timer.CurrentDay;

    // Salva a imagem associada ao paciente e ao dia.
    ExamsSaveData.Save(
        ultrasoundManager.resultImage.sprite,
        isDefault
            ? BodyArea.BodyRegion.Empty
            : ultrasoundManager.CurrentRegion,
        patient,
        currentDay
    );

    ExamsSaveData.IsDefaultUltrasoundImage = isDefault;

    // Toca o som de clique do FMOD.
    RuntimeManager.PlayOneShot(somClick);

    // Exibe a confirmação, se configurada.
    if (confirmText != null)
    {
        confirmText.SetActive(true);

        if (confirmCoroutine != null)
            StopCoroutine(confirmCoroutine);

        confirmCoroutine = StartCoroutine(HideConfirmation());
    }

    Debug.Log(
        $"Imagem salva para {patient.patientName}, " +
        $"dia {currentDay}: {ultrasoundManager.CurrentRegion}"
    );
}

    private IEnumerator HideConfirmation()
    {
        yield return new WaitForSeconds(textTime);

        confirmText.SetActive(false);

        confirmCoroutine = null;
    }
}

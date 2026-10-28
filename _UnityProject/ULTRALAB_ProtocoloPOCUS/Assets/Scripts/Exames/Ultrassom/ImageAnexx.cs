
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ImageAnexx : MonoBehaviour
{
    [Header("UI do prontuário")]
    [SerializeField] private Image anexxImage;
    [SerializeField] private TMP_Text requestedRegionText;

    [Header("Validação")]
    public bool approved;

    [Header("Tutorial")]
    [Tooltip("Marque se este componente pertence ao prontuário do tutorial.")]
    [SerializeField] private bool tutorialMode;

    [Tooltip("Imagem correta do coração exigida no tutorial.")]
    [SerializeField] private Sprite tutorialHeartImage;

    [Header("Cena")]
    [SerializeField] private string uti;
    [SerializeField] private Timer timerReference;

    private void OnEnable()
    {
        UpdateUI();
    }

    public void OnAnexx()
    {
        UpdateUI();
        AudioManager.Instance?.PlayBack();
    }

    public void UpdateUI()
    {
        if (tutorialMode)
        {
            if (requestedRegionText != null)
                requestedRegionText.text = "Coração";

            bool hasHeartAttachment =
                ExamsSaveData.SavedImage != null &&
                (tutorialHeartImage == null ||
                 ExamsSaveData.SavedImage == tutorialHeartImage) &&
                ExamsSaveData.SavedExam.HasValue &&
                ExamsSaveData.SavedExam.Value == BodyArea.BodyRegion.Heart;

            if (anexxImage != null)
            {
                anexxImage.sprite = hasHeartAttachment
                    ? ExamsSaveData.SavedImage
                    : null;

                anexxImage.enabled = hasHeartAttachment;
                anexxImage.color = Color.white;
            }

            return;
        }

        PatientData patient = CurrentPatient.Data;
        int day = GetCurrentDay();

        BodyArea.BodyRegion region = patient != null
            ? PatientDailyState.GetUltrasoundRegion(patient, day)
            : BodyArea.BodyRegion.Empty;

        if (requestedRegionText != null)
            requestedRegionText.text = GetRegionName(region);

        bool hasCurrentAttachment =
            patient != null &&
            ExamsSaveData.SavedImage != null &&
            ExamsSaveData.OwnerPatient == patient &&
            ExamsSaveData.SavedDay == day;

        if (anexxImage != null)
        {
            anexxImage.sprite = hasCurrentAttachment
                ? ExamsSaveData.SavedImage
                : null;

            anexxImage.enabled = hasCurrentAttachment;
            anexxImage.color = Color.white;
        }
    }

    public void OnConfirm()
    {
        approved = false;

        // Tutorial: exige somente o exame do coração.
        if (tutorialMode)
        {
            bool validHeart =
                ExamsSaveData.SavedImage != null &&
                (tutorialHeartImage == null ||
                 ExamsSaveData.SavedImage == tutorialHeartImage) &&
                !ExamsSaveData.IsDefaultUltrasoundImage &&
                ExamsSaveData.SavedExam.HasValue &&
                ExamsSaveData.SavedExam.Value == BodyArea.BodyRegion.Heart;

            if (!validHeart)
            {
                Debug.LogWarning(
                    "No tutorial, anexe o resultado correto do ultrassom do coração."
                );

                UpdateUI();
                return;
            }

            approved = true;
            Debug.Log("Tutorial: ultrassom do coração validado.");

            LoadUTIScene();
            return;
        }

        // Validação normal: exige paciente e dia.
        PatientData patient = CurrentPatient.Data;

        if (patient == null)
        {
            Debug.LogWarning(
                "Não há paciente selecionado para validar o prontuário."
            );
            return;
        }

        int day = GetCurrentDay();

        BodyArea.BodyRegion requiredRegion =
            PatientDailyState.GetUltrasoundRegion(patient, day);

        bool hasCurrentAttachment =
            ExamsSaveData.SavedImage != null &&
            ExamsSaveData.OwnerPatient == patient &&
            ExamsSaveData.SavedDay == day;

        if (requiredRegion == BodyArea.BodyRegion.Empty)
        {
            if (hasCurrentAttachment)
            {
                Debug.LogWarning(
                    "Nenhum ultrassom foi solicitado. " +
                    "Remova o exame anexado antes de confirmar."
                );

                UpdateUI();
                return;
            }

            approved = true;
            Debug.Log(
                "Prontuário validado: nenhum ultrassom solicitado."
            );

            LoadUTIScene();
            return;
        }

        if (!hasCurrentAttachment ||
            ExamsSaveData.IsDefaultUltrasoundImage ||
            !ExamsSaveData.SavedExam.HasValue)
        {
            Debug.LogWarning(
                "Anexe o resultado do ultrassom solicitado " +
                "para o paciente e dia atuais."
            );

            UpdateUI();
            return;
        }

        Sprite requiredImage =
            PatientDailyState.GetUltrasoundImage(patient, day);

        bool correct =
            requiredImage != null &&
            ExamsSaveData.SavedImage == requiredImage &&
            ExamsSaveData.SavedExam.Value == requiredRegion;

        if (!correct)
        {
            Debug.LogWarning(
                "Exame incorreto. Confira a imagem salva " +
                "e a região anatômica."
            );

            UpdateUI();
            return;
        }

        approved = true;
        Debug.Log(
            "Ultrassom validado para o paciente e dia atuais."
        );

        LoadUTIScene();
    }

    private string GetRegionName(BodyArea.BodyRegion region)
    {
        switch (region)
        {
            case BodyArea.BodyRegion.Heart:
                return "Coração";

            case BodyArea.BodyRegion.Lung1:
            case BodyArea.BodyRegion.Lung2:
                return "Pulmão";

            case BodyArea.BodyRegion.Bladder:
                return "Bexiga";

            default:
                return "Vazio";
        }
    }

    private int GetCurrentDay()
    {
        Timer activeTimer = timerReference != null
            ? timerReference
            : Timer.Instance;

        return activeTimer != null
            ? Mathf.Max(1, activeTimer.CurrentDay)
            : 1;
    }

    private void LoadUTIScene()
    {
        PauseController.SetPause(false);

        if (PlayerReferences.Instance != null)
        {
            PlayerReferences.Instance.RefreshReferences();
            PlayerReferences.Instance.EnablePlayer();
        }

        if (loading_manager.instance != null)
            loading_manager.instance.SwitchScene(uti);
        else
            SceneManager.LoadScene(uti);
    }
}

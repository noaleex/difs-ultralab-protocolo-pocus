using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ImageAnexx : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image anexxImage;
    public bool approved;
    [SerializeField] private string uti;

    public void OnAnexx()
    {
        UpdateUI();

        AudioManager.Instance?.PlayBack();
    }

    public void UpdateUI()
    {
        if (anexxImage == null)
            return;
        if (ExamsSaveData.SavedImage == null)
        {
            anexxImage.sprite = null;
            return;
        }

        int day = Timer.Instance != null ? Mathf.Max(1, Timer.Instance.CurrentDay) : 1;
        if (ExamsSaveData.OwnerPatient != CurrentPatient.Data || ExamsSaveData.SavedDay != day)
        {
            anexxImage.sprite = null;
            return;
        }

        anexxImage.sprite = ExamsSaveData.SavedImage;
        anexxImage.color = Color.white;
    }

    public void OnConfirm()
    {
        if (CurrentPatient.Data == null || ExamsSaveData.OwnerPatient != CurrentPatient.Data)
        {
            Debug.LogWarning("A imagem anexada não pertence ao paciente atual.");
            approved = false;
            return;
        }

        int day = Timer.Instance != null ? Mathf.Max(1, Timer.Instance.CurrentDay) : 1;
        if (ExamsSaveData.SavedDay != day)
        {
            Debug.LogWarning("A imagem anexada pertence a outro dia.");
            approved = false;
            return;
        }

        Sprite requiredImage = PatientDailyState.GetUltrasoundImage(CurrentPatient.Data, day);
        if (requiredImage != null)
        {
            BodyArea.BodyRegion requiredRegion = PatientDailyState.GetUltrasoundRegion(CurrentPatient.Data, day);
            bool correct = !ExamsSaveData.IsDefaultUltrasoundImage
                && ExamsSaveData.SavedImage == requiredImage
                && ExamsSaveData.SavedExam == requiredRegion;
            approved = correct;
            if (!correct)
            {
                Debug.Log("A imagem anexada não corresponde ao exame solicitado para este paciente hoje.");
                return;
            }
            LoadUTIScene();
            return;
        }

        switch (ExamsSaveData.SavedExam)
        {
            case BodyArea.BodyRegion.Heart:
                Debug.Log("Imagem do Coração confirmada.");
                approved = true;
                LoadUTIScene();
                break;

            case BodyArea.BodyRegion.Lung1:
                Debug.Log("Imagem do Pulmão confirmada.");
                approved = false;
                break;

            case BodyArea.BodyRegion.Lung2:
                Debug.Log("Imagem do Pulmão confirmada.");
                approved = false;
                break;

            case BodyArea.BodyRegion.Bladder:
                Debug.Log("Imagem da Bexiga confirmada.");
                approved = false;
                break;

            case BodyArea.BodyRegion.Empty:
                Debug.Log("Imagem vazia confirmada.");
                approved = false;
                break;

            default:
                Debug.Log("Anexe uma imagem para confirmar.");
                break;
        }
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

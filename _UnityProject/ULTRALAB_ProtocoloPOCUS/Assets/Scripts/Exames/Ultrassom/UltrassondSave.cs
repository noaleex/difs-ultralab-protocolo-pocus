using System.Collections;
using UnityEngine;

public class UltrassondSave : MonoBehaviour
{
    [SerializeField] private UltrasoundManager ultrasoundManager;
    [SerializeField] private GameObject confirmText;

    [SerializeField] private float textTime = 1.5f;

    private Coroutine confirmCoroutine;

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

        bool isDefault =
            ultrasoundManager.resultImage.sprite == ultrasoundManager.defaultImage;

        ExamsSaveData.Save(
            ultrasoundManager.resultImage.sprite,
            isDefault
                ? BodyArea.BodyRegion.Empty
                : ultrasoundManager.CurrentRegion
        );

        ExamsSaveData.IsDefaultUltrasoundImage = isDefault;

        // Ativa o Panel
        confirmText.SetActive(true);

        // Reinicia o contador caso salve novamente antes de desaparecer
        if (confirmCoroutine != null)
            StopCoroutine(confirmCoroutine);

        confirmCoroutine = StartCoroutine(HideConfirmation());

        Debug.Log($"Imagem salva: {ExamsSaveData.SavedExam}");
    }

    private IEnumerator HideConfirmation()
    {
        yield return new WaitForSeconds(textTime);

        confirmText.SetActive(false);

        confirmCoroutine = null;
    }
}
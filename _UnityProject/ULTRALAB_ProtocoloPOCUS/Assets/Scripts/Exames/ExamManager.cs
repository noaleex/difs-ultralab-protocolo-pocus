using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExamManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Image characterFullImage;
    [SerializeField] private Image characterFullImage2;
    [SerializeField] private Image characterFullImage3;

    [Header("Tutorial")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject table;
    [SerializeField] private Button button;

    private void Start()
    {
        if (CurrentPatient.ClinicalData == null)
        {
            return;
        }

        if (characterImage != null) characterImage.sprite = CurrentPatient.ClinicalData.characterSprite;
        if (characterFullImage != null) characterFullImage.sprite = CurrentPatient.ClinicalData.characterFullSprite;
        if (characterFullImage2 != null) characterFullImage2.sprite = CurrentPatient.ClinicalData.characterFullSprite;
        if (characterFullImage3 != null) characterFullImage3.sprite = CurrentPatient.ClinicalData.characterFullSprite;
        //infoText.text = CurrentPatient.Data.caso;

        if (CurrentPatient.ClinicalData.tutorial)
        {
            if (table != null) table.SetActive(false);
            if (button != null) button.interactable = false;
            if (backgroundImage != null) backgroundImage.sprite = CurrentPatient.ClinicalData.backgroundSprite;
        }
    }
}
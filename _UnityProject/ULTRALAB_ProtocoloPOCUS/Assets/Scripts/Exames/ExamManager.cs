using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExamManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image characterImage;
    [SerializeField] private Image characterFullImage;
    [SerializeField] private Image characterFullImage2;

    [Header("Tutorial")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject table;
    [SerializeField] private Button button;

    private void Start()
    {
        if (CurrentPatient.Data == null)
        {
            return;
        }

        characterImage.sprite = CurrentPatient.Data.characterSprite;
        characterFullImage.sprite = CurrentPatient.Data.characterFullSprite;
        characterFullImage2.sprite = CurrentPatient.Data.characterFullSprite;
        //infoText.text = CurrentPatient.Data.caso;

        if (CurrentPatient.Data.tutorial)
        {
            table.SetActive(false);
            button.interactable = false;
            backgroundImage.sprite = CurrentPatient.Data.backgroundSprite;
        }
    }
}
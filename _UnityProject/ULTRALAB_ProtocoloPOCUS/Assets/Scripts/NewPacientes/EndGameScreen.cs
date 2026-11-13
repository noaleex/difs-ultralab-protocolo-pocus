using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;

    [SerializeField] private GameObject finalBom;
    [SerializeField] private GameObject finalRuim;

    private bool isFinalBom = false;

    private void Start()
    {
        Time.timeScale = 1f;

        bool positive = GameEndingManager.LastEnding ==
                        GameEndingManager.EndingType.Positive;

        isFinalBom = positive;

        if (titleText != null)
        {
            titleText.text = positive ? "Fim" : "Fim";
        }

        if (messageText != null)
        {
            messageText.text = positive
                ? "Você conseguiu cuidar de todos os pacientes!"
                : "Infelizmente, um paciente não resistiu.";
        }

        if (finalBom != null)
            finalBom.SetActive(positive);

        if (finalRuim != null)
            finalRuim.SetActive(!positive);
    }
}

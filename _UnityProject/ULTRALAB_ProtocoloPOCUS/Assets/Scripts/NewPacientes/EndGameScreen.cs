using TMPro;
using UnityEngine;

public class EndGameScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;

    private void Start()
    {
        Time.timeScale = 1f;
        bool positive = GameEndingManager.LastEnding == GameEndingManager.EndingType.Positive;

        if (titleText != null)
            titleText.text = positive ? "Parabéns!" : "Fim de jogo";

        if (messageText != null)
        {
            messageText.text = positive
                ? "Você conseguiu tratar os três pacientes e todos receberam alta."
                : "Um dos pacientes chegou a 0 pontos. O atendimento foi encerrado.";
        }
    }
}

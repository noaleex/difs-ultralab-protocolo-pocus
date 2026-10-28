using UnityEngine;
using UnityEngine.UI;

/// <summary>Conecta os botões do painel instanciado ao NPC correto, sem referências fixas ao NPC 1.</summary>
public sealed class PatientExamPanelController : MonoBehaviour
{
    [SerializeField] private Button examButton;
    [SerializeField] private Button conductButton;
    [SerializeField] private Button closeButton;

    private OpenExams owner;

    public void Initialize(OpenExams patient)
    {
        owner = patient;
        if (examButton != null)
        {
            examButton.onClick.RemoveListener(OpenExam);
            examButton.onClick.AddListener(OpenExam);
        }
        if (conductButton != null)
        {
            conductButton.onClick.RemoveListener(OpenConduct);
            conductButton.onClick.AddListener(OpenConduct);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void OpenExam()
    {
        if (owner != null) owner.OpenExam();
    }

    private void OpenConduct()
    {
        if (owner != null) owner.OpenConduta();
    }

    private void ClosePanel()
    {
        if (owner != null) owner.ClosePanel();
    }
}

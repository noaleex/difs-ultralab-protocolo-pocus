using UnityEngine;

/// <summary>
/// Instancia o painel de ações para o NPC que recebeu a interação.
/// Coloque este componente em um objeto ativo da cena (por exemplo, Canvas).
/// </summary>
public sealed class PatientPanelUIHost : MonoBehaviour
{
    public static PatientPanelUIHost Instance { get; private set; }

    [Header("Painel de ações - prefab")]
    [SerializeField] private GameObject examPanelPrefab;
    [SerializeField] private Transform panelRoot;

    [Header("Painéis compartilhados da cena")]
    [SerializeField] private GameObject conductPanel;
    [SerializeField] private MedicalData medicalDataUI;

    private OpenExams currentOwner;
    private GameObject currentPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[PatientPanelUIHost] Há mais de um host de painéis na cena.", this);
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool OpenPanel(OpenExams owner)
    {
        if (owner == null) return false;
        if (examPanelPrefab == null || panelRoot == null)
        {
            Debug.LogError("[PatientPanelUIHost] Atribua Exam Panel Prefab e Panel Root no Inspector.", this);
            return false;
        }

        if (currentPanel != null) Destroy(currentPanel);
        currentOwner = owner;
        currentPanel = Instantiate(examPanelPrefab, panelRoot, false);
        currentPanel.SetActive(true);

        owner.BindRuntimePanels(currentPanel, conductPanel, medicalDataUI);

        PatientExamPanelController controller = currentPanel.GetComponent<PatientExamPanelController>();
        if (controller == null)
        {
            Debug.LogError("O prefab do painel precisa ter PatientExamPanelController na raiz.", currentPanel);
            Destroy(currentPanel);
            currentPanel = null;
            currentOwner = null;
            return false;
        }

        controller.Initialize(owner);
        return true;
    }

    public void ClosePanel(OpenExams owner)
    {
        if (owner == null || owner != currentOwner) return;
        if (currentPanel != null) Destroy(currentPanel);
        currentPanel = null;
        currentOwner = null;
    }

    /// <summary>Use no botão Voltar/Fechar do painel de conduta compartilhado.</summary>
    public void CloseCurrentPatientPanel()
    {
        if (currentOwner != null) currentOwner.ClosePanel();
    }
}

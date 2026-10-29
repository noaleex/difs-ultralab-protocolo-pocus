using TMPro;
using UnityEngine;

public sealed class PatientPanelUIHost : MonoBehaviour
{
    public static PatientPanelUIHost Instance { get; private set; }

    [Header("Prefab do painel de ações")]
    [SerializeField] private GameObject examPanelPrefab;
    [SerializeField] private Transform panelRoot;

    [Header("Painéis compartilhados da cena")]
    [SerializeField] private GameObject conductPanel;
    [SerializeField] private MedicalData medicalDataUI;

    [Header("Referências compartilhadas da alimentação")]
    [SerializeField] private ChoiceFood choiceFood;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text requiredFoodText;

    private OpenExams currentOwner;
    private GameObject currentPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "[PatientPanelUIHost] Já existe outro host na cena.",
                this
            );

            enabled = false;
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool OpenPanel(OpenExams owner)
    {
        if (owner == null)
            return false;

        if (examPanelPrefab == null || panelRoot == null)
        {
            Debug.LogError(
                "[PatientPanelUIHost] Atribua Exam Panel Prefab e Panel Root.",
                this
            );

            return false;
        }

        if (currentPanel != null)
            Destroy(currentPanel);

        currentOwner = owner;

        currentPanel = Instantiate(
            examPanelPrefab,
            panelRoot,
            false
        );

        currentPanel.SetActive(true);

        owner.BindRuntimePanels(
            currentPanel,
            conductPanel,
            medicalDataUI
        );

        PatientExamPanelController controller =
            currentPanel.GetComponent<PatientExamPanelController>();

        if (controller == null)
        {
            Debug.LogError(
                "[PatientPanelUIHost] A prefab precisa ter PatientExamPanelController na raiz.",
                currentPanel
            );

            Destroy(currentPanel);
            currentPanel = null;
            currentOwner = null;
            return false;
        }

        controller.Initialize(owner);

        FoodDeliveryButton foodButton =
            currentPanel.GetComponentInChildren<FoodDeliveryButton>(true);

        if (foodButton != null)
        {
            foodButton.Initialize(
                choiceFood,
                feedbackText,
                requiredFoodText,
                currentPanel
            );
        }
        else
        {
            Debug.LogWarning(
                "[PatientPanelUIHost] Não foi encontrado FoodDeliveryButton na prefab.",
                currentPanel
            );
        }

        return true;
    }

    public void ClosePanel(OpenExams owner)
    {
        if (owner == null || owner != currentOwner)
            return;

        if (currentPanel != null)
            Destroy(currentPanel);

        currentPanel = null;
        currentOwner = null;
    }

    public void CloseCurrentPatientPanel()
    {
        if (currentOwner != null)
            currentOwner.ClosePanel();
    }
}

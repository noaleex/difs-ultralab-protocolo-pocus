using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolsManager : MonoBehaviour
{
    [Serializable]
    public class AreaFeedback
    {
        public BodyArea bodyArea;
    }

    [Serializable]
    public class ToolConfiguration
    {
        public ToolsSelected.ToolsType toolType;
        public Image handImage;
        public AreaFeedback[] areaFeedbacks;
        public List<TMP_Text> feedbackTexts = new List<TMP_Text>();
    }

    public static ToolsManager Instance;

    [Header("Imagem da Mão")]
    public Image handDisplay;
    public Sprite toolImage;

    [Header("Faixa de BPM")]
    [Min(1)] public int minimumBpm = 60;
    [Min(1)] public int maximumBpm = 100;

    [Header("Faixa de pressão arterial (mmHg)")]
    [Min(1)] public int minimumSystolicPressure = 100;
    [Min(1)] public int maximumSystolicPressure = 140;
    [Min(1)] public int minimumDiastolicPressure = 60;
    [Min(1)] public int maximumDiastolicPressure = 90;

    [Header("Configuração dos Utensílios")]
    public ToolConfiguration[] tools;

    private RectTransform toolRect;
    private ToolsSelected.ToolsType currentTool = ToolsSelected.ToolsType.None;
    private BodyArea activeArea;

    private void Awake()
    {
        Instance = this;

        if (handDisplay != null)
        {
            toolRect = handDisplay.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("O componente Image que exibe a mão não foi configurado.");
        }

        if (toolImage == null)
            Debug.LogError("O sprite padrão da mão do ToolsManager não foi configurado.");
    }

    private void Start()
    {
        ApplySprite(handDisplay, toolImage);
        ClearFeedbackTexts();
        SetDefaultFeedback();
    }

    public void SelectTool(ToolsSelected.ToolsType tool)
    {
        currentTool = tool;
        activeArea = null;

        ToolConfiguration configuration = FindToolConfiguration(tool);

        if (tool == ToolsSelected.ToolsType.None)
        {
            ApplySprite(handDisplay, toolImage);
        }
        else if (configuration == null)
        {
            Debug.LogWarning("Não há configuração para o utensílio selecionado: " + tool);
            ApplySprite(handDisplay, toolImage);
        }
        else if (configuration.handImage == null)
        {
            Debug.LogWarning("A imagem da mão não foi configurada para o utensílio: " + tool);
            ApplySprite(handDisplay, toolImage);
        }
        else if (handDisplay != null)
        {
            ApplySprite(handDisplay, configuration.handImage.sprite);
        }

        if (toolRect != null)
            CheckToolPosition(toolRect.position);
        else
            SetDefaultFeedback();
    }

    public void CheckToolPosition(Vector2 toolPosition)
    {
        ToolConfiguration configuration = FindToolConfiguration(currentTool);

        if (currentTool != ToolsSelected.ToolsType.None &&
            configuration != null &&
            configuration.areaFeedbacks != null)
        {
            foreach (AreaFeedback areaFeedback in configuration.areaFeedbacks)
            {
                if (areaFeedback == null || areaFeedback.bodyArea == null)
                    continue;

                if (areaFeedback.bodyArea.ContainsPoint(toolPosition))
                {
                    if (activeArea != areaFeedback.bodyArea)
                        ShowRandomToolFeedback(areaFeedback.bodyArea);
                    return;
                }
            }
        }

        SetDefaultFeedback();
    }

    private ToolConfiguration FindToolConfiguration(ToolsSelected.ToolsType tool)
    {
        if (tools == null)
            return null;

        foreach (ToolConfiguration configuration in tools)
        {
            if (configuration != null && configuration.toolType == tool)
                return configuration;
        }

        return null;
    }

    private void SetDefaultFeedback()
    {
        activeArea = null;
    }

    private void ShowRandomToolFeedback(BodyArea area)
    {
        activeArea = area;

        switch (currentTool)
        {
            case ToolsSelected.ToolsType.Estetoscopio:
                ShowRandomBpm();
                break;

            case ToolsSelected.ToolsType.Esfigmomanometro:
                ShowRandomBloodPressure();
                break;

            case ToolsSelected.ToolsType.Gel:
                SetToolFeedbackText("Gel aplicado");
                break;
        }
    }

    private void ShowRandomBpm()
    {
        if (minimumBpm > maximumBpm)
        {
            Debug.LogError("O BPM mínimo não pode ser maior que o BPM máximo.");
            return;
        }

        int bpm = UnityEngine.Random.Range(minimumBpm, maximumBpm + 1);
        SetToolFeedbackText(bpm + " BPM");
    }

    private void ShowRandomBloodPressure()
    {
        int firstValidSystolic = Mathf.Max(minimumSystolicPressure, minimumDiastolicPressure + 1);
        int lastValidDiastolic = Mathf.Min(maximumDiastolicPressure, maximumSystolicPressure - 1);

        if (firstValidSystolic > maximumSystolicPressure ||
            minimumDiastolicPressure > lastValidDiastolic)
        {
            Debug.LogError("As faixas configuradas de pressão arterial não permitem uma medida válida.");
            return;
        }

        int systolic = UnityEngine.Random.Range(firstValidSystolic, maximumSystolicPressure + 1);
        int diastolicMaximum = Mathf.Min(maximumDiastolicPressure, systolic - 1);
        int diastolic = UnityEngine.Random.Range(minimumDiastolicPressure, diastolicMaximum + 1);

        SetToolFeedbackText(systolic + "/" + diastolic + " mmHg");
    }

    private void ClearFeedbackTexts()
    {
        if (tools == null)
            return;

        foreach (ToolConfiguration configuration in tools)
        {
            if (configuration == null || configuration.feedbackTexts == null)
                continue;

            foreach (TMP_Text text in configuration.feedbackTexts)
            {
                if (text != null)
                    text.text = string.Empty;
            }
        }
    }

    private void SetToolFeedbackText(string value)
    {
        ToolConfiguration configuration = FindToolConfiguration(currentTool);
        if (configuration == null || configuration.feedbackTexts == null)
        {
            Debug.LogWarning("Não há textos de feedback configurados para o utensílio: " + currentTool);
            return;
        }

        bool hasAssignedText = false;
        foreach (TMP_Text text in configuration.feedbackTexts)
        {
            if (text == null)
                continue;

            text.text = value;
            hasAssignedText = true;
        }

        if (!hasAssignedText)
            Debug.LogWarning("Adicione pelo menos um TMP_Text à lista de feedbacks do utensílio: " + currentTool);
    }

    private void ApplySprite(Image target, Sprite source)
    {
        if (target != null)
            target.sprite = source;
    }
}

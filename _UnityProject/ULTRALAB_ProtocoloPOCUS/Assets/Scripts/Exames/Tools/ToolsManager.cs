using System;
using UnityEngine;
using UnityEngine.UI;

public class ToolsManager : MonoBehaviour
{
    [Serializable]
    public class AreaFeedback
    {
        public BodyArea bodyArea;
        public Sprite feedbackImage;
    }

    [Serializable]
    public class ToolConfiguration
    {
        public ToolsSelected.ToolsType toolType;
        public Sprite handSprite;
        public AreaFeedback[] areaFeedbacks;
    }

    public static ToolsManager Instance;

    [Header("Imagem da Mão")]
    public Image toolImage;
    public Sprite defaultHandSprite;

    [Header("Feedback no Canvas")]
    public Image feedbackImage;
    public Sprite defaultFeedbackImage;

    [Header("Configuração dos Utensílios")]
    public ToolConfiguration[] tools;

    private RectTransform toolRect;
    private ToolsSelected.ToolsType currentTool = ToolsSelected.ToolsType.None;

    private void Awake()
    {
        Instance = this;

        if (toolImage != null)
        {
            toolRect = toolImage.GetComponent<RectTransform>();
            if (defaultHandSprite == null)
                defaultHandSprite = toolImage.sprite;
        }
        else
        {
            Debug.LogError("A imagem da mão do ToolsManager não foi configurada.");
        }

        if (feedbackImage != null && defaultFeedbackImage == null)
            defaultFeedbackImage = feedbackImage.sprite;
        else if (feedbackImage == null)
            Debug.LogError("A imagem de feedback do ToolsManager não foi configurada.");
    }

    private void Start()
    {
        if (feedbackImage != null)
            feedbackImage.sprite = defaultFeedbackImage;

        if (toolImage != null && defaultHandSprite != null)
            toolImage.sprite = defaultHandSprite;
    }

    public void SelectTool(ToolsSelected.ToolsType tool)
    {
        currentTool = tool;

        ToolConfiguration configuration = FindToolConfiguration(tool);

        if (tool == ToolsSelected.ToolsType.None)
        {
            if (toolImage != null)
                toolImage.sprite = defaultHandSprite;
        }
        else if (configuration == null)
        {
            Debug.LogWarning("Não há configuração para o utensílio selecionado: " + tool);
            if (toolImage != null)
                toolImage.sprite = defaultHandSprite;
        }
        else if (configuration.handSprite == null)
        {
            Debug.LogWarning("O sprite da mão não foi configurado para o utensílio: " + tool);
            if (toolImage != null)
                toolImage.sprite = defaultHandSprite;
        }
        else if (toolImage != null)
        {
            toolImage.sprite = configuration.handSprite;
        }

        if (toolRect != null)
            CheckToolPosition(toolRect.position);
        else
            SetDefaultFeedback();
    }

    public void CheckToolPosition(Vector2 toolPosition)
    {
        if (feedbackImage == null)
            return;

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
                    feedbackImage.sprite = areaFeedback.feedbackImage != null
                        ? areaFeedback.feedbackImage
                        : defaultFeedbackImage;
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
        if (feedbackImage != null)
            feedbackImage.sprite = defaultFeedbackImage;
    }
}

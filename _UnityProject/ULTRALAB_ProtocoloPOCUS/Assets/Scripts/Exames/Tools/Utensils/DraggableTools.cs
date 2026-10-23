using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableTools : MonoBehaviour, IDragHandler
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;

        if (ToolsManager.Instance != null)
            ToolsManager.Instance.CheckToolPosition(rectTransform.position);
    }
}
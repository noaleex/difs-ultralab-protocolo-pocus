using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class PlayerDepthSorting2D : MonoBehaviour
{
    [SerializeField] private int baseOrder = 0;
    [SerializeField] private float orderPerWorldUnit = 100f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null)
            return;

        float feetY = spriteRenderer.bounds.min.y;
        spriteRenderer.sortingOrder = baseOrder + Mathf.RoundToInt(-feetY * orderPerWorldUnit);
    }
}
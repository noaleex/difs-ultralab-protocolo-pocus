using UnityEngine;

public class BodyArea : MonoBehaviour
{
    public enum BodyRegion
    {
        Heart,
        Lung1,
        Lung2,
        Bladder,
        Empty
    }

    public BodyRegion region;
    public Sprite correctImage;

    private Collider2D areaCollider;

    private void Awake()
    {
        areaCollider = GetComponent<Collider2D>();
    }

    public bool ContainsPoint(Vector2 worldPosition)
    {
        return areaCollider != null && areaCollider.OverlapPoint(worldPosition);
    }
}
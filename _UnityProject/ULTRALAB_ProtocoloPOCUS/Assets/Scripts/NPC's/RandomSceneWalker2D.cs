using UnityEngine;

public class RandomSceneWalker2D : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D walkArea;
    [SerializeField] private float moveSpeed = 1.6f;
    [SerializeField] private float bodyRadius = 0.35f;
    [SerializeField] private float destinationTolerance = 0.12f;
    [SerializeField] private float minPause = 0.5f;
    [SerializeField] private float maxPause = 1.5f;
    [SerializeField] private float minDestinationDistance = 2f;
    [SerializeField] private int destinationAttempts = 80;

    private Collider2D ownCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 destination;
    private bool hasDestination;
    private float pauseUntil;
    private float lastInputX;
    private float lastInputY;
    private int originalSortingOrder;

    private void Awake()
    {
        ownCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;

        if (walkArea == null)
            walkArea = GameObject.Find("T1")?.GetComponent<PolygonCollider2D>();
    }

    private void Update()
    {
        UpdateDepthOrdering();

        NPC npc = GetComponent<NPC>();
        if (PauseController.IsGamePaused || pause_manager.isGamePaused || (npc != null && npc.IsDialogueActive))
        {
            SetIdle();
            return;
        }

        if (Time.time < pauseUntil)
        {
            SetIdle();
            return;
        }

        if (!hasDestination && !TryChooseDestination())
        {
            pauseUntil = Time.time + 0.5f;
            SetIdle();
            return;
        }

        Vector2 current = transform.position;
        Vector2 delta = destination - current;
        float distance = delta.magnitude;

        if (distance <= destinationTolerance)
        {
            hasDestination = false;
            pauseUntil = Time.time + Random.Range(minPause, maxPause);
            SetIdle();
            return;
        }

        Vector2 direction = delta / distance;
        float step = moveSpeed * Time.deltaTime;
        float castDistance = Mathf.Min(step, distance);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(current, bodyRadius, direction, castDistance);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider == ownCollider || hit.collider == walkArea)
                continue;

            hasDestination = false;
            SetIdle();
            return;
        }

        transform.position = current + direction * castDistance;
        SetFacing(direction);
        if (animator != null)
        {
            animator.SetFloat("horizontal", lastInputX);
            animator.SetFloat("vertical", lastInputY);
            animator.SetBool("isWalking", true);
        }
    }

    private void UpdateDepthOrdering()
    {
        if (spriteRenderer == null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SpriteRenderer playerRenderer = player != null ? player.GetComponent<SpriteRenderer>() : null;
        if (playerRenderer == null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
            return;
        }

        bool playerIsInFront = playerRenderer.bounds.min.y < spriteRenderer.bounds.min.y;
        spriteRenderer.sortingOrder = playerIsInFront
            ? playerRenderer.sortingOrder - 1
            : playerRenderer.sortingOrder + 1;
    }

    private bool TryChooseDestination()
    {
        if (walkArea == null)
            return false;

        Bounds bounds = walkArea.bounds;
        for (int attempt = 0; attempt < destinationAttempts; attempt++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y));

            if (!walkArea.OverlapPoint(candidate))
                continue;

            if (Vector2.Distance(transform.position, candidate) < minDestinationDistance)
                continue;

            Vector2 direction = candidate - (Vector2)transform.position;
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, bodyRadius, direction.normalized, direction.magnitude);
            bool blocked = false;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider == ownCollider || hit.collider == walkArea)
                    continue;

                blocked = true;
                break;
            }

            if (blocked)
                continue;

            destination = candidate;
            hasDestination = true;
            return true;
        }

        return false;
    }

    private void SetFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            lastInputX = Mathf.Sign(direction.x);
            lastInputY = 0f;
        }
        else
        {
            lastInputX = 0f;
            lastInputY = Mathf.Sign(direction.y);
        }
    }

    private void SetIdle()
    {
        if (animator == null)
            return;

        animator.SetBool("isWalking", false);
        animator.SetFloat("horizontal", lastInputX);
        animator.SetFloat("vertical", lastInputY);
    }
}

using System.Collections.Generic;
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
    [SerializeField] private float pathGridSize = 0.75f;
    [SerializeField] private float collisionSkin = 0.04f;

    private Collider2D ownCollider;
    private BoxCollider2D boxCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private NPC npc;
    private readonly List<Vector2> path = new List<Vector2>();

    private Vector2 currentWaypoint;
    private bool hasPath;
    private float pauseUntil;
    private float lastInputX;
    private float lastInputY;
    private int originalSortingOrder;

    private void Awake()
    {
        ownCollider = GetComponent<Collider2D>();
        boxCollider = ownCollider as BoxCollider2D;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        npc = GetComponent<NPC>();
        originalSortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;

        if (walkArea == null)
            walkArea = GameObject.Find("T1")?.GetComponent<PolygonCollider2D>();
    }

    private void Update()
    {
        UpdateDepthOrdering();

        if (PauseController.IsGamePaused ||
            pause_manager.isGamePaused ||
            (npc != null && npc.IsDialogueActive))
        {
            StopAtCurrentPosition();
            return;
        }

        if (Time.time < pauseUntil)
        {
            StopAtCurrentPosition();
            return;
        }

        if (!hasPath && !TryChoosePath())
        {
            pauseUntil = Time.time + 0.5f;
            StopAtCurrentPosition();
            return;
        }

        Vector2 current = transform.position;
        Vector2 delta = currentWaypoint - current;
        float distance = delta.magnitude;

        if (distance <= destinationTolerance)
        {
            if (path.Count > 0)
            {
                currentWaypoint = path[0];
                path.RemoveAt(0);
                delta = currentWaypoint - current;
                distance = delta.magnitude;
            }

            if (distance <= destinationTolerance && path.Count == 0)
            {
                hasPath = false;
                pauseUntil = Time.time + Random.Range(minPause, maxPause);
                StopAtCurrentPosition();
                return;
            }
        }

        if (distance <= 0.001f)
        {
            hasPath = false;
            pauseUntil = Time.time + 0.25f;
            StopAtCurrentPosition();
            return;
        }

        Vector2 direction = delta / distance;
        float step = Mathf.Min(moveSpeed * Time.deltaTime, distance);

        if (!CanMove(direction, step))
        {
            hasPath = false;
            path.Clear();
            pauseUntil = Time.time + Random.Range(0.15f, 0.4f);
            StopAtCurrentPosition();
            return;
        }

        transform.position = current + direction * step;
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

    private bool TryChoosePath()
    {
        if (walkArea == null)
            walkArea = GameObject.Find("T1")?.GetComponent<PolygonCollider2D>();

        if (walkArea == null)
            return false;

        Bounds bounds = walkArea.bounds;

        for (int attempt = 0; attempt < destinationAttempts; attempt++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y));

            if (!IsWalkable(candidate) ||
                Vector2.Distance(transform.position, candidate) < minDestinationDistance)
            {
                continue;
            }

            if (TryFindPath((Vector2)transform.position, candidate, out List<Vector2> foundPath))
            {
                path.Clear();
                path.AddRange(foundPath);

                if (path.Count == 0)
                    continue;

                currentWaypoint = path[0];
                path.RemoveAt(0);
                hasPath = true;
                return true;
            }
        }

        return false;
    }

    private bool TryFindPath(Vector2 startPosition, Vector2 targetPosition, out List<Vector2> result)
    {
        result = new List<Vector2>();

        Bounds bounds = walkArea.bounds;
        int width = Mathf.CeilToInt(bounds.size.x / pathGridSize) + 1;
        int height = Mathf.CeilToInt(bounds.size.y / pathGridSize) + 1;
        int total = width * height;

        bool[] walkable = new bool[total];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = ToIndex(x, y, width);
                walkable[index] = IsWalkable(GridToWorld(x, y, bounds));
            }
        }

        Vector2Int start = FindNearestWalkable(WorldToGrid(startPosition, bounds), walkable, width, height);
        Vector2Int goal = FindNearestWalkable(WorldToGrid(targetPosition, bounds), walkable, width, height);

        if (start.x < 0 || goal.x < 0)
            return false;

        int startIndex = ToIndex(start.x, start.y, width);
        int goalIndex = ToIndex(goal.x, goal.y, width);

        float[] scores = new float[total];
        int[] cameFrom = new int[total];
        bool[] closed = new bool[total];
        List<int> open = new List<int>();

        for (int i = 0; i < total; i++)
        {
            scores[i] = float.PositiveInfinity;
            cameFrom[i] = -1;
        }

        scores[startIndex] = 0f;
        open.Add(startIndex);

        while (open.Count > 0)
        {
            int currentIndex = GetLowestScoreNode(open, scores, goal, width);
            if (currentIndex == goalIndex)
            {
                BuildPath(cameFrom, currentIndex, startIndex, width, bounds, result);
                return result.Count > 0;
            }

            open.Remove(currentIndex);
            closed[currentIndex] = true;

            int currentX = currentIndex % width;
            int currentY = currentIndex / width;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nextX = currentX + dx;
                    int nextY = currentY + dy;

                    if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height)
                        continue;

                    int nextIndex = ToIndex(nextX, nextY, width);
                    if (!walkable[nextIndex] || closed[nextIndex])
                        continue;

                    if (dx != 0 && dy != 0)
                    {
                        if (!walkable[ToIndex(currentX + dx, currentY, width)] ||
                            !walkable[ToIndex(currentX, currentY + dy, width)])
                        {
                            continue;
                        }
                    }

                    float moveCost = dx != 0 && dy != 0 ? 1.4142f : 1f;
                    float newScore = scores[currentIndex] + moveCost;

                    if (newScore < scores[nextIndex])
                    {
                        scores[nextIndex] = newScore;
                        cameFrom[nextIndex] = currentIndex;

                        if (!open.Contains(nextIndex))
                            open.Add(nextIndex);
                    }
                }
            }
        }

        return false;
    }

    private void BuildPath(
        int[] cameFrom,
        int currentIndex,
        int startIndex,
        int width,
        Bounds bounds,
        List<Vector2> result)
    {
        List<Vector2> reversed = new List<Vector2>();

        while (currentIndex != startIndex && currentIndex >= 0)
        {
            int x = currentIndex % width;
            int y = currentIndex / width;
            reversed.Add(GridToWorld(x, y, bounds));
            currentIndex = cameFrom[currentIndex];
        }

        for (int i = reversed.Count - 1; i >= 0; i--)
            result.Add(reversed[i]);
    }

    private int GetLowestScoreNode(List<int> open, float[] scores, Vector2Int goal, int width)
    {
        int best = open[0];
        float bestValue = scores[best] + Heuristic(best, goal, width);

        for (int i = 1; i < open.Count; i++)
        {
            int candidate = open[i];
            float value = scores[candidate] + Heuristic(candidate, goal, width);

            if (value < bestValue)
            {
                best = candidate;
                bestValue = value;
            }
        }

        return best;
    }

    private float Heuristic(int index, Vector2Int goal, int width)
    {
        int x = index % width;
        int y = index / width;
        return Mathf.Abs(x - goal.x) + Mathf.Abs(y - goal.y);
    }

    private Vector2Int FindNearestWalkable(Vector2Int origin, bool[] walkable, int width, int height)
    {
        if (origin.x >= 0 && origin.x < width &&
            origin.y >= 0 && origin.y < height &&
            walkable[ToIndex(origin.x, origin.y, width)])
        {
            return origin;
        }

        int maxRadius = Mathf.Max(width, height);
        for (int radius = 1; radius < maxRadius; radius++)
        {
            for (int y = origin.y - radius; y <= origin.y + radius; y++)
            {
                for (int x = origin.x - radius; x <= origin.x + radius; x++)
                {
                    if (x < 0 || x >= width || y < 0 || y >= height)
                        continue;

                    if (walkable[ToIndex(x, y, width)])
                        return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    private Vector2Int WorldToGrid(Vector2 position, Bounds bounds)
    {
        return new Vector2Int(
            Mathf.RoundToInt((position.x - bounds.min.x) / pathGridSize),
            Mathf.RoundToInt((position.y - bounds.min.y) / pathGridSize));
    }

    private Vector2 GridToWorld(int x, int y, Bounds bounds)
    {
        return new Vector2(
            bounds.min.x + x * pathGridSize,
            bounds.min.y + y * pathGridSize);
    }

    private int ToIndex(int x, int y, int width)
    {
        return y * width + x;
    }

    private bool IsWalkable(Vector2 position)
    {
        if (walkArea == null || !walkArea.OverlapPoint(position))
            return false;

        Vector2 center = position + GetColliderOffset();
        Vector2 size = GetColliderSize();

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);
        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == ownCollider || hit == walkArea || hit.isTrigger)
                continue;

            return false;
        }

        return true;
    }

    private bool CanMove(Vector2 direction, float distance)
    {
        Vector2 center = (Vector2)transform.position + GetColliderOffset();
        Vector2 size = GetColliderSize();
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            center,
            size,
            0f,
            direction,
            distance + collisionSkin);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null ||
                hit.collider == ownCollider ||
                hit.collider == walkArea ||
                hit.collider.isTrigger)
            {
                continue;
            }

            // Larissa can start with her collider touching a tilemap edge.
            // Allow only the step that exits that initial overlap; future
            // steps remain protected by the projected-position test below.
            if (hit.distance <= collisionSkin &&
                (Vector2.Dot(hit.normal, direction) > 0.05f ||
                 IsWalkable((Vector2)transform.position + direction * distance)))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private Vector2 GetColliderOffset()
    {
        if (boxCollider != null)
            return boxCollider.offset;

        return Vector2.zero;
    }

    private Vector2 GetColliderSize()
    {
        if (boxCollider != null)
            return boxCollider.size * 0.92f;

        return Vector2.one * (bodyRadius * 2f);
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

    private void StopAtCurrentPosition()
    {
        if (animator == null)
            return;

        animator.SetBool("isWalking", false);
        animator.SetFloat("horizontal", lastInputX);
        animator.SetFloat("vertical", lastInputY);
    }
}

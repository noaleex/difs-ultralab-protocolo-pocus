using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class MirandaTableWander : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Rect tableWalkBounds = new Rect(5f, -11.13f, 5f, 0.05f);
    [SerializeField, Min(0.01f)] private float walkSpeed = 1.35f;
    [SerializeField, Min(0f)] private float pauseMin = 0.2f;
    [SerializeField, Min(0f)] private float pauseMax = 0.65f;
    [SerializeField, Min(0.05f)] private float walkMin = 6f;
    [SerializeField, Min(0.05f)] private float walkMax = 12f;
    [SerializeField, Min(0f)] private float edgePadding = 0.04f;
    [SerializeField, Min(0f)] private float collisionSkin = 0.03f;

    private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];
    private ContactFilter2D castFilter;
    private float stateTimer;
    private float targetX;
    private int direction = 1;
    private bool walking;
    private bool initialized;
    private string currentAnimationState;

    private void Awake()
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        castFilter = new ContactFilter2D { useTriggers = false, useLayerMask = true };
        castFilter.SetLayerMask(Physics2D.AllLayers);
        initialized = true;
        BeginPause();
    }

    private void OnEnable()
    {
        if (initialized && !walking) BeginPause();
    }

    private void FixedUpdate()
    {
        if (body == null || PauseController.IsGamePaused) return;

        if (!walking)
        {
            stateTimer -= Time.fixedDeltaTime;
            if (stateTimer <= 0f) BeginWalk();
            return;
        }

        float nextX = Mathf.MoveTowards(body.position.x, targetX, walkSpeed * Time.fixedDeltaTime);
        float deltaX = nextX - body.position.x;
        if (Mathf.Abs(deltaX) <= 0.0001f)
        {
            BeginPause();
            return;
        }

        Vector2 moveDirection = new Vector2(Mathf.Sign(deltaX), 0f);
        if (IsBlocked(moveDirection, Mathf.Abs(deltaX) + collisionSkin))
        {
            if (!TryReverseAtObstacle())
                BeginPause();
            return;
        }

        body.MovePosition(new Vector2(nextX, tableWalkBounds.center.y));
        stateTimer -= Time.fixedDeltaTime;
        if (stateTimer <= 0f || Mathf.Abs(body.position.x - targetX) <= 0.01f)
            BeginPause();
    }

    private bool IsBlocked(Vector2 moveDirection, float distance)
    {
        int hitCount = body.Cast(moveDirection, castFilter, castHits, distance);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = castHits[i].collider;
            if (hit == null || hit.transform.IsChildOf(transform) || hit.isTrigger) continue;
            if (IsWalkSurfaceFurniture(hit)) continue;
            return true;
        }
        return false;
    }

    private static bool IsWalkSurfaceFurniture(Collider2D hit)
    {
        string objectName = hit.gameObject.name;
        return objectName == "05_forniture(not interactables)" ||
               objectName == "06_forniture_sobreposição";
    }

    private bool TryReverseAtObstacle()
    {
        float colliderHalfWidth = GetPhysicalHalfWidth();
        float minX = tableWalkBounds.xMin + colliderHalfWidth + edgePadding;
        float maxX = tableWalkBounds.xMax - colliderHalfWidth - edgePadding;
        if (maxX <= minX) return false;

        int reverseDirection = -direction;
        Vector2 reverseVector = new Vector2(reverseDirection, 0f);
        if (IsBlocked(reverseVector, collisionSkin + 0.08f))
            return false;

        direction = reverseDirection;
        targetX = Mathf.Clamp(body.position.x + direction * Random.Range(walkMin, walkMax), minX, maxX);
        stateTimer = Mathf.Max(0.1f, Mathf.Abs(targetX - body.position.x) / walkSpeed);
        SetAnimation(direction < 0 ? "WalkLeft" : "WalkRight");
        return true;
    }

    private void BeginPause()
    {
        walking = false;
        stateTimer = Random.Range(pauseMin, pauseMax);
        SetAnimation("Idle");
    }

    private void BeginWalk()
    {
        float colliderHalfWidth = GetPhysicalHalfWidth();
        float minX = tableWalkBounds.xMin + colliderHalfWidth + edgePadding;
        float maxX = tableWalkBounds.xMax - colliderHalfWidth - edgePadding;
        if (maxX <= minX)
        {
            BeginPause();
            return;
        }

        direction = Random.value < 0.5f ? -1 : 1;
        float distance = Random.Range(walkMin, walkMax);
        float edgeTarget = direction < 0 ? minX : maxX;
        targetX = Mathf.Clamp(body.position.x + direction * distance, minX, maxX);
        if (Mathf.Abs(targetX - body.position.x) < 0.05f)
            targetX = edgeTarget;
        stateTimer = Mathf.Max(0.1f, Mathf.Abs(targetX - body.position.x) / walkSpeed);
        walking = true;
        SetAnimation(direction < 0 ? "WalkLeft" : "WalkRight");
    }

    private float GetPhysicalHalfWidth()
    {
        float halfWidth = 0.3f;
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(true))
        {
            if (collider == null || collider.isTrigger || !collider.enabled) continue;
            halfWidth = Mathf.Max(halfWidth, collider.bounds.extents.x);
        }
        return halfWidth;
    }

    private void SetAnimation(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (currentAnimationState == stateName && state.IsName(stateName)) return;

        currentAnimationState = stateName;
        animator.Play(stateName, 0, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(tableWalkBounds.center.x, tableWalkBounds.center.y, transform.position.z);
        Vector3 size = new Vector3(tableWalkBounds.width, Mathf.Max(tableWalkBounds.height, 0.05f), 0.02f);
        Gizmos.DrawWireCube(center, size);
    }
}

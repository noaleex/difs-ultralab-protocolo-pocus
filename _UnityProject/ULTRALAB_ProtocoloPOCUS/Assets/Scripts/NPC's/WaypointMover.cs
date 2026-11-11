using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTime = 1f;
    public bool loopWaypoints = true;

    [Header("Navegação")]
    [SerializeField] private float stoppingDistance = 0.08f;
    [SerializeField] private float sampleDistance = 1f;
    [SerializeField] private float repathInterval = 0.25f;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting;
    private Animator animator;
    private NavMeshAgent agent;
    private NPC npc;
    private float nextRepathTime;
    private Vector3 currentDestination;
    private bool hasDestination;
    private NavMeshTriangulation navMeshTriangulation;

    private float lastInputY;
    private float lastInputX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        npc = GetComponent<NPC>();

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("WaypointMover precisa de um NavMeshAgent.", this);
            enabled = false;
            return;
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, sampleDistance, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
            {
                Debug.LogError("Larissa não está sobre uma NavMesh válida.", this);
                enabled = false;
                return;
            }
        }

        navMeshTriangulation = NavMesh.CalculateTriangulation();
        if (navMeshTriangulation.vertices == null || navMeshTriangulation.vertices.Length < 3 ||
            navMeshTriangulation.indices == null || navMeshTriangulation.indices.Length < 3)
        {
            Debug.LogError("A NavMesh da UTI não possui uma área navegável triangulada.", this);
            enabled = false;
            return;
        }

        if (waypointParent != null && waypointParent.childCount > 0)
        {
            waypoints = new Transform[waypointParent.childCount];
            for (int i = 0; i < waypointParent.childCount; i++)
                waypoints[i] = waypointParent.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool dialogueActive = npc != null && npc.IsDialogueActive;
        if (PauseController.IsGamePaused || pause_manager.isGamePaused || dialogueActive || isWaiting)
        {
            StopAgentAndAnimateIdle();
            return;
        }

        MoveToNextWaypoint();
    }

    void MoveToNextWaypoint()
    {
        agent.isStopped = false;
        if (!hasDestination && !TryChooseNavMeshDestination())
        {
            TryChooseWaypointDestination();
            if (!hasDestination)
                return;
        }

        if (Time.time >= nextRepathTime)
        {
            agent.SetDestination(currentDestination);
            nextRepathTime = Time.time + repathInterval;
        }

        Vector2 velocity = agent.velocity;
        if (velocity.sqrMagnitude > 0.01f)
            SetFacingFromVelocity(velocity);

        animator.SetFloat("horizontal", lastInputX);
        animator.SetFloat("vertical", lastInputY);
        animator.SetBool("isWalking", velocity.sqrMagnitude > 0.01f);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        StopAgentAndAnimateIdle();

        yield return new WaitForSeconds(waitTime);
        
        hasDestination = false;
        isWaiting = false;
    }

    private bool TryChooseNavMeshDestination()
    {
        int triangleCount = navMeshTriangulation.indices.Length / 3;
        for (int attempt = 0; attempt < 40; attempt++)
        {
            int triangle = Random.Range(0, triangleCount) * 3;
            Vector3 a = navMeshTriangulation.vertices[navMeshTriangulation.indices[triangle]];
            Vector3 b = navMeshTriangulation.vertices[navMeshTriangulation.indices[triangle + 1]];
            Vector3 c = navMeshTriangulation.vertices[navMeshTriangulation.indices[triangle + 2]];

            float u = Random.value;
            float v = Random.value;
            if (u + v > 1f)
            {
                u = 1f - u;
                v = 1f - v;
            }

            Vector3 candidate = a + u * (b - a) + v * (c - a);
            if (Vector2.Distance(transform.position, candidate) < 1.5f)
                continue;

            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(candidate, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                currentDestination = candidate;
                hasDestination = true;
                return true;
            }
        }

        return false;
    }

    private void TryChooseWaypointDestination()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        for (int attempt = 0; attempt < waypoints.Length; attempt++)
        {
            Transform target = waypoints[currentWaypointIndex];
            currentWaypointIndex = loopWaypoints
                ? (currentWaypointIndex + 1) % waypoints.Length
                : Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);

            NavMeshHit hit;
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.SamplePosition(target.position, out hit, sampleDistance, NavMesh.AllAreas) &&
                agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                currentDestination = hit.position;
                hasDestination = true;
                return;
            }
        }
    }

    private void SetFacingFromVelocity(Vector2 velocity)
    {
        if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
        {
            lastInputX = Mathf.Sign(velocity.x);
            lastInputY = 0f;
        }
        else if (Mathf.Abs(velocity.y) > 0.01f)
        {
            lastInputX = 0f;
            lastInputY = Mathf.Sign(velocity.y);
        }
    }

    private void StopAgentAndAnimateIdle()
    {
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        if (animator == null)
            return;

        animator.SetBool("isWalking", false);
        animator.SetFloat("horizontal", lastInputX);
        animator.SetFloat("vertical", lastInputY);
    }
}

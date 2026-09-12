using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using FMODUnity;
using FMOD.Studio;

public class PlayerMovementNavMash : MonoBehaviour
{
    private NavMeshAgent agent;
    private Camera mainCamera;
    private PlayerAction controls;

    [SerializeField] private Animator animator;

    public EventReference FootstepSound;
    private EventInstance footstepInstance;

    private bool isWalking;
    private bool isPointerOverUI;

    private bool isAndroid;

    [Header("NavMesh")]
    [SerializeField] private float sampleDistance = 2f;

    // Controle do toque no Android
    private bool isTouching;
    private int activeTouchId = -1;


    private void Awake()
    {
        isAndroid = Application.platform == RuntimePlatform.Android;

        if (!isAndroid)
        {
            NavMeshAgent nav = GetComponent<NavMeshAgent>();

            if (nav != null)
                nav.enabled = false;

            this.enabled = false;
            return;
        }

        controls = new PlayerAction();

        controls.UI.Click.performed += OnTouchPressed;
        controls.UI.Click.canceled += OnTouchReleased;
    }


    private void OnEnable()
    {
        if (controls != null)
            controls.Enable();
    }


    private void OnDisable()
    {
        if (controls != null)
            controls.Disable();

        StopMovement();
        StopFootstepSound();
    }


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        FindCamera();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
    }


    private void Update()
    {
        if (mainCamera == null)
            FindCamera();

        CheckPointerOverUI();

        // Enquanto o dedo estiver pressionado,
        if (isTouching)
        {
            UpdateTouchMovement();
        }

        UpdateAnimation();
        UpdateFootstepSound();
        CheckInvalidDestination();
    }


    private void FindCamera()
    {
        mainCamera = Camera.main;
    }

    private void OnTouchPressed(InputAction.CallbackContext ctx)
    {
        if (!isAndroid)
            return;

        if (Pointer.current == null)
            return;

        CheckPointerOverUI();

        if (isPointerOverUI)
            return;

        isTouching = true;

        MoveToPointer();
    }


    private void OnTouchReleased(InputAction.CallbackContext ctx)
    {
        if (!isAndroid)
            return;

        isTouching = false;
        activeTouchId = -1;

        StopMovement();
    }


    private void UpdateTouchMovement()
    {
        if (!isTouching)
            return;

        if (Pointer.current == null)
            return;

        CheckPointerOverUI();

        // não continua movimentando o personagem.
        if (isPointerOverUI)
        {
            StopMovement();
            return;
        }

        MoveToPointer();
    }


    private void MoveToPointer()
    {
        if (mainCamera == null)
            FindCamera();

        if (mainCamera == null || agent == null)
            return;

        if (Pointer.current == null)
            return;

        Vector2 pointerPosition =
            Pointer.current.position.ReadValue();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(pointerPosition);

        worldPosition.z = 0;


        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            worldPosition,
            out hit,
            sampleDistance,
            NavMesh.AllAreas))
        {
            StopMovement();
            return;
        }


        NavMeshPath path = new NavMeshPath();

        if (!agent.CalculatePath(hit.position, path))
        {
            StopMovement();
            return;
        }


        if (path.status != NavMeshPathStatus.PathComplete)
        {
            StopMovement();
            return;
        }


        agent.SetDestination(hit.position);
    }


    private void CheckPointerOverUI()
    {
        if (EventSystem.current == null ||
            Pointer.current == null)
        {
            isPointerOverUI = false;
            return;
        }


        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);


        pointerData.position =
            Pointer.current.position.ReadValue();


        var results =
            new System.Collections.Generic.List<RaycastResult>();


        EventSystem.current.RaycastAll(
            pointerData,
            results
        );


        isPointerOverUI = results.Count > 0;
    }

    private void CheckInvalidDestination()
    {
        if (agent == null)
            return;

        if (!agent.hasPath)
            return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            StopMovement();
    }


    private void StopMovement()
    {
        if (agent != null)
            agent.ResetPath();

        StopAnimation();
    }


    private void UpdateAnimation()
    {
        if (agent == null || animator == null)
            return;

        Vector2 moveDir =
            agent.velocity.normalized;

        animator.SetFloat(
            "horizontal",
            moveDir.x
        );

        animator.SetFloat(
            "vertical",
            moveDir.y
        );
    }


    public void StopAnimation()
    {
        if (animator == null)
            return;

        animator.SetFloat("horizontal", 0);
        animator.SetFloat("vertical", 0);
    }


    private void UpdateFootstepSound()
    {
        if (agent == null)
            return;

        bool shouldWalk =
            agent.velocity.sqrMagnitude > 0.01f;


        if (shouldWalk && !isWalking)
        {
            PlayFootstepSound();
        }
        else if (!shouldWalk && isWalking)
        {
            StopFootstepSound();
        }
    }


    private void PlayFootstepSound()
    {
        if (!FootstepSound.IsNull)
        {
            footstepInstance =
                RuntimeManager.CreateInstance(
                    FootstepSound
                );


            footstepInstance.set3DAttributes(
                RuntimeUtils.To3DAttributes(gameObject)
            );


            footstepInstance.start();

            isWalking = true;
        }
    }


    private void StopFootstepSound()
    {
        if (footstepInstance.isValid())
        {
            footstepInstance.stop(
                FMOD.Studio.STOP_MODE.IMMEDIATE
            );

            footstepInstance.release();

            isWalking = false;
        }
    }

    private void OnDestroy()
    {
        if (controls != null)
        {
            controls.UI.Click.performed -= OnTouchPressed;
            controls.UI.Click.canceled -= OnTouchReleased;

            controls.Dispose();
        }

        StopFootstepSound();
    }
}
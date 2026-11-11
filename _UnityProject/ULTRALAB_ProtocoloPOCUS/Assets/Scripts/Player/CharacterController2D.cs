using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    Rigidbody2D rigidbody2d;
    [SerializeField] float speed = 2f;
    Vector2 motionVector;
    PlayerAction controls;
    Animator animator;

    public EventReference FoodStep;
    private EventInstance footstepInstance;
    private bool isWalking = false;
    private float stopGraceTimer = 0f;
    private const float StopGraceTime = 0.2f;

    void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        EnsureControls();
        animator = GetComponent<Animator>();
    }

    private void EnsureControls()
    {
        if (controls != null)
            return;

        controls = new PlayerAction();

        controls.Player.Move.performed += ctx => motionVector = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => motionVector = Vector2.zero;
    }

    void OnEnable()
    {
        EnsureControls();
        controls.Enable();
    }

    void OnDisable()
    {
        if (controls != null)
            controls.Disable();

        StopFootstepSound();
    }

    void OnDestroy()
    {
        StopFootstepSound();
        if (footstepInstance.isValid())
        {
            footstepInstance.release();
        }
    }

    void Update()
    {
        if (animator != null)
        {
            animator.SetFloat("horizontal", motionVector.x);
            animator.SetFloat("vertical", motionVector.y);
        }

        UpdateFootstepSound();
    }

    void FixedUpdate()
    {
        if (PauseController.IsGamePaused)
            return;

        Move();
    }

    void Move()
    {
        if (rigidbody2d != null)
            rigidbody2d.linearVelocity = motionVector * speed;
    }

    private void UpdateFootstepSound()
    {
        bool shouldWalk = motionVector.sqrMagnitude > 0.01f;

        if (shouldWalk)
        {
            stopGraceTimer = StopGraceTime;
            if (!isWalking)
            {
                PlayFootstepSound();
            }
        }
        else
        {
            if (stopGraceTimer > 0f)
            {
                stopGraceTimer -= Time.deltaTime;
            }
            else if (isWalking)
            {
                StopFootstepSound();
            }
        }
    }

    private void PlayFootstepSound()
    {
        if (!FoodStep.IsNull)
        {
            if (PauseController.IsGamePaused)
                return;

            footstepInstance = RuntimeManager.CreateInstance(FoodStep);
            footstepInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            footstepInstance.start();
            isWalking = true;
        }
    }

    private void StopFootstepSound()
    {
        if (footstepInstance.isValid())
        {
            footstepInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            footstepInstance.release();
            isWalking = false;
        }
    }
}
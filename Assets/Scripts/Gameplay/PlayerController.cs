using UnityEngine;
using UnityEngine.InputSystem;

// PBI 4 (Amber's tasks, written ahead): left/right movement and jumping.
// Put this on the player together with Rigidbody2D, a Collider2D and PlayerRespawn.
//
// Controls come from the project's Input Actions (InputSystem_Actions):
//   Move = A/D or Left/Right arrows (also gamepad stick), Jump = Space (also gamepad south button).
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpVelocity = 11f;

    [Tooltip("Seconds after walking off a ledge during which a jump still works.")]
    [SerializeField] private float coyoteTime = 0.1f;

    [Tooltip("Seconds a jump press is remembered before landing.")]
    [SerializeField] private float jumpBufferTime = 0.1f;

    /// <summary>True while a menu or reflection question is open; the player can't move.</summary>
    public static bool InputLocked { get; set; }

    public bool IsGrounded { get; private set; }

    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction jumpAction;
    private ContactFilter2D groundFilter;
    private float lastGroundedTime = float.NegativeInfinity;
    private float lastJumpPressedTime = float.NegativeInfinity;
    private bool waitForNeutralInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Ground = any solid contact whose surface faces upward.
        groundFilter = new ContactFilter2D { useTriggers = false };
        groundFilter.SetNormalAngle(45f, 135f);

        InputActionAsset actions = InputSystem.actions;
        moveAction = actions != null ? actions.FindAction("Player/Move") : null;
        jumpAction = actions != null ? actions.FindAction("Player/Jump") : null;

        // Fallback if the project-wide actions asset is ever removed.
        if (moveAction == null)
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        }
        if (jumpAction == null)
            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void Update()
    {
        if (!InputLocked && jumpAction.WasPressedThisFrame())
            lastJumpPressedTime = Time.time;
    }

    private void FixedUpdate()
    {
        IsGrounded = rb.IsTouching(groundFilter);
        if (IsGrounded) lastGroundedTime = Time.time;

        float x = InputLocked ? 0f : moveAction.ReadValue<Vector2>().x;

        // After a menu/question closes, ignore movement until the keys are let go.
        // Otherwise a letter still held from typing an answer (A or D) moves the player.
        if (InputLocked) waitForNeutralInput = true;
        else if (waitForNeutralInput)
        {
            if (Mathf.Abs(x) > 0.01f) x = 0f;
            else waitForNeutralInput = false;
        }

        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        if (jumpBuffered && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            lastJumpPressedTime = float.NegativeInfinity;
            lastGroundedTime = float.NegativeInfinity;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => InputLocked = false;
}

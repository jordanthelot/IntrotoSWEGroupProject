
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.15f;

    [Header("Character Visuals")]
    public SpriteRenderer characterSprite;
    public Animator characterAnimator;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    [Header("Footsteps")]
    public AudioClip footstepSound;
    public float footstepInterval = 0.3f;

    private float footstepTimer = 0f;

    // Optional: lets an automated test drive the player instead of the keyboard.
    // Leave null in the game (the keyboard is used).
    public IPlayerInputSource inputOverride;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool jumpRequested;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Left and right movement
        horizontalInput = inputOverride != null
            ? inputOverride.Horizontal
            : Input.GetAxisRaw("Horizontal");

        // Check whether the player is on the ground
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Jump when Space is pressed
        bool jumpPressed = inputOverride != null
            ? inputOverride.ConsumeJumpPressed()
            : Input.GetButtonDown("Jump");

        if (jumpPressed && isGrounded)
        {
            jumpRequested = true;
        }

        // Flip the character when moving left
        if (horizontalInput != 0 && characterSprite != null)
        {
            characterSprite.flipX = horizontalInput < 0;
        }

        // Update animations
        if (characterAnimator != null)
        {
            characterAnimator.SetFloat(
                "Speed",
                Mathf.Abs(horizontalInput)
            );

            characterAnimator.SetBool(
                "IsGrounded",
                isGrounded
            );
            HandleFootsteps();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y
        );


        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            // Play the jump sound
            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }

            jumpRequested = false;
        }

    }

    private void HandleFootsteps()
    {
        bool isMoving = Mathf.Abs(horizontalInput) > 0.1f;

        if (isMoving && isGrounded)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                if (audioSource != null && footstepSound != null)
                {
                    audioSource.PlayOneShot(footstepSound, 0.4f);
                }

                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // Reset when standing still or in the air
            footstepTimer = 0f;
        }
    }

}

using System;
using UnityEngine;
using UnityEngine.Events;

// PBI 4: Configure unlimited lives / respawning.
// Put this on the player (the object tagged "Player" that has the Rigidbody2D).
//
// Unlimited lives: there is no life counter. Every fall respawns the player.
// The player respawns at the last checkpoint reached (set by Checkpoint.cs),
// or at their starting position if no checkpoint has been reached yet.
//
// A fall is detected two ways:
//   1. The player touches a KillZone trigger (put KillZone.cs under gaps/pits).
//   2. Safety net: the player drops below Fall Death Y, in case a gap has no KillZone.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    [Tooltip("If the player falls below this height they respawn, even without a KillZone.")]
    [SerializeField] private float fallDeathY = -20f;

    [Tooltip("Runs after every respawn (e.g. reset animation, play a sound).")]
    public UnityEvent onRespawned;

    /// <summary>Fires after every respawn. Useful for other scripts (game manager, UI).</summary>
    public event Action<PlayerRespawn> Respawned;

    public Vector2 RespawnPoint { get; private set; }
    public int RespawnCount { get; private set; }

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        RespawnPoint = transform.position; // level start until a checkpoint is reached
    }

    private void FixedUpdate()
    {
        if (rb.position.y < fallDeathY) Respawn();
    }

    /// <summary>Called by Checkpoint when the player reaches it.</summary>
    public void SetRespawnPoint(Vector2 point) => RespawnPoint = point;

    public void Respawn()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = RespawnPoint;
        transform.position = RespawnPoint;
        RespawnCount++;

        onRespawned?.Invoke();
        Respawned?.Invoke(this);
    }
}

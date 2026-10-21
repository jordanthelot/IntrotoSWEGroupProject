using System;
using UnityEngine;

// PBI 4: Configure unlimited lives / respawning.
// Goes on the Player next to Amber's PlayerCheckpoint, which remembers the last checkpoint.
//
// Unlimited lives: there is no life counter. Every fall sends the player back to the
// last checkpoint reached (or the start of the level if none yet).
//
// A fall is detected two ways:
//   1. The player touches a KillZone trigger (placed under the level / gaps).
//   2. Safety net: the player drops below Fall Death Y, in case a gap has no KillZone.
[RequireComponent(typeof(PlayerCheckpoint))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    [Tooltip("If the player falls below this height they respawn, even without a KillZone.")]
    public float fallDeathY = -30f;

    /// <summary>Fires after every respawn (for UI, sounds, a game manager, etc.).</summary>
    public event Action<PlayerRespawn> Respawned;

    /// <summary>How many times the player has respawned this session. There is no limit.</summary>
    public int RespawnCount { get; private set; }

    private PlayerCheckpoint checkpoint;
    private Rigidbody2D rb;

    private void Awake()
    {
        checkpoint = GetComponent<PlayerCheckpoint>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (rb.position.y < fallDeathY) Respawn();
    }

    public void Respawn()
    {
        checkpoint.Respawn();          // Amber's: moves the player to the saved checkpoint
        transform.position = rb.position;
        RespawnCount++;
        Respawned?.Invoke(this);
    }
}

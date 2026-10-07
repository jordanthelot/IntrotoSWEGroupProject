using System;
using UnityEngine;
using UnityEngine.Events;

// PBI 5: Detect when the player reaches a checkpoint.
// Put this on each checkpoint object (Amber's checkpoint objects). The object needs a
// Collider2D set to "Is Trigger" (done automatically when you add this component).
//
// When the player touches it:
//   - the player's respawn point moves here (PBI 4),
//   - Checkpoint.AnyReached fires (for scripts that listen for every checkpoint),
//   - onReached fires (hook Jordan's "trigger reflection activity" here in the Inspector).
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Unique ID used for saving progress. Defaults to the object's name.")]
    [SerializeField] private string checkpointId = "";

    [Tooltip("Optional: where the player respawns. If empty, the checkpoint's own position is used.")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("Only fire the first time the player reaches this checkpoint.")]
    [SerializeField] private bool triggerOnce = true;

    [Tooltip("Tag on the player object.")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Runs when the player reaches this checkpoint (e.g. open the reflection question).")]
    public UnityEvent onReached;

    /// <summary>Fires for every checkpoint the player reaches.</summary>
    public static event Action<Checkpoint> AnyReached;

    public string Id => string.IsNullOrWhiteSpace(checkpointId) ? gameObject.name : checkpointId;
    public bool IsReached { get; private set; }
    public Vector2 RespawnPosition => respawnPoint != null ? (Vector2)respawnPoint.position : (Vector2)transform.position;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hit = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        if (!hit.CompareTag(playerTag)) return;
        if (IsReached && triggerOnce) return;

        IsReached = true;
        if (hit.TryGetComponent(out PlayerRespawn player)) player.SetRespawnPoint(RespawnPosition);

        Debug.Log($"Checkpoint reached: {Id}");
        onReached?.Invoke();
        AnyReached?.Invoke(this);
    }

    // Static events survive between Play sessions when domain reload is off; clear them on start.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => AnyReached = null;
}

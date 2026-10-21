using UnityEngine;

// PBI 4: Put this on a trigger collider under the level or under a gap.
// The player respawns at their last checkpoint when they fall into it.
[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    // Runs when the component is first added in the Editor: makes the collider a trigger.
    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void Awake() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hit = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;

        if (hit.TryGetComponent(out PlayerRespawn respawn)) respawn.Respawn();
        else if (hit.TryGetComponent(out PlayerCheckpoint checkpoint)) checkpoint.Respawn();
    }
}

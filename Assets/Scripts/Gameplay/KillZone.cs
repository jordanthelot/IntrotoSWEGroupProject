using UnityEngine;

// PBI 4: Put this on a trigger collider under each gap or pit.
// Anything with PlayerRespawn that touches it respawns.
[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    // Runs when the component is first added in the Editor: makes the collider a trigger.
    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject hit = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
        if (hit.TryGetComponent(out PlayerRespawn player)) player.Respawn();
    }
}

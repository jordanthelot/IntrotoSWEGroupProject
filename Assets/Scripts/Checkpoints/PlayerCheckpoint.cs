
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCheckpoint : MonoBehaviour
{
    private Vector3 respawnPosition;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        respawnPosition = transform.position;
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        // Preserve the player's original Z position
        respawnPosition = new Vector3(
            checkpointPosition.x,
            checkpointPosition.y,
            transform.position.z
        );

        Debug.Log("New respawn position saved: " + respawnPosition);
    }

    public void Respawn()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = new Vector2(
            respawnPosition.x,
            respawnPosition.y
        );

        Debug.Log("Player respawned at checkpoint.");
    }
}

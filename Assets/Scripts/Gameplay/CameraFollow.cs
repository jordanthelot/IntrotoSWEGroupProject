using UnityEngine;

// Keeps the camera on the player. Finds the object tagged "Player" if no target is set.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);
    [SerializeField] private float smoothing = 8f;

    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;
            target = player.transform;
        }

        Vector3 goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
    }
}

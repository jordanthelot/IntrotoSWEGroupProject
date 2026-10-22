
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    public float smoothSpeed = 5f;
    public float xOffset = 0f;
    public float yOffset = 1f;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = new Vector3(
            target.position.x + xOffset,
            target.position.y + yOffset,
            -10f
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Mathf.Clamp01(smoothSpeed * Time.deltaTime)
        );
    }
}



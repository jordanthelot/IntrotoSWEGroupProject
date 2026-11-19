
using UnityEngine;

public class SkyBackgroundFit : MonoBehaviour
{
    private Camera mainCamera;

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null || !mainCamera.orthographic)
            return;

        // Follow the camera without changing its Z depth.
        Vector3 position = transform.position;
        position.x = mainCamera.transform.position.x;
        position.y = mainCamera.transform.position.y;
        transform.position = position;

        // Fit the 576 x 324 sky sprites to the camera.
        float height = mainCamera.orthographicSize * 2f;
        float width = height * mainCamera.aspect;

        SpriteRenderer spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null ||
            spriteRenderer.sprite == null)
            return;

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        float scale = Mathf.Max(
            width / spriteSize.x,
            height / spriteSize.y
        );

        transform.localScale =
            new Vector3(scale, scale, 1f);
    }
}


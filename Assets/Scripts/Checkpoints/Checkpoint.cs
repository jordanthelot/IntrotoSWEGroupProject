
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Appearance")]
    public Color inactiveColor = Color.yellow;
    public Color activeColor = Color.green;
    [Header("Reflection Question")]
    public ReflectionUIManager reflectionUI;

    [TextArea(2, 4)]
    public string reflectionQuestion =
        "How do you usually feel after spending a long time scrolling through social media?";

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        GetComponent<Collider2D>().isTrigger = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = inactiveColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated)
            return;

        // Only activate when the Player enters
        if (!other.CompareTag("Player"))
            return;

        isActivated = true;

        // Save the player's current checkpoint
        PlayerCheckpoint playerCheckpoint =
            other.GetComponent<PlayerCheckpoint>();

        if (playerCheckpoint != null)
        {
            playerCheckpoint.SetCheckpoint(transform.position);
        }

        // Change checkpoint appearance
        if (spriteRenderer != null)
        {
            spriteRenderer.color = activeColor;
        }

        Debug.Log("Checkpoint activated: " + gameObject.name);
        if (reflectionUI != null)
        {
            reflectionUI.ShowReflection(reflectionQuestion);
        }
    }
}


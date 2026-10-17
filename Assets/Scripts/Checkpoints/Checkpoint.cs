
using UnityEngine;

//implemented most of amber's old code, but instead of this file calling on reflectionUI to open the reflection, it calls on reflectionmanager
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{

    [Header("Checkpoint Identity")]
    public string checkpointId = "test_checkpoint_1";
    //dummy variable initialized with test_checkpoint_1- later on, it'll hold checkpointid that'll be given to reflection manager

    //this part is still preserved from amber's original code
    [Header("Checkpoint Appearance")]
    public Color inactiveColor = Color.yellow;
    public Color activeColor = Color.green;
    [Header("Reflection")]
    public ReflectionManager reflectionManager;
    //now calls reflectionmanager to handle creating the reflection

    private SpriteRenderer spriteRenderer;
    private bool isActivated;
    void Awake()//same code from amber's previous file
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
        if (!other.CompareTag("Player"))
            return;

        // Set the respawn position and color once.
        if (!isActivated)
        {
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

            Debug.Log("Checkpoint activated: " + checkpointId);
        }

    
        // The manager prevents already-answered questions from repeating.
        if (reflectionManager != null)
        {
            reflectionManager.OnCheckpointReached(checkpointId);//reflection manager now shows reflection
        }
        //Reflectionmanager opens the reflectionUI
}
}



using UnityEngine;

public enum CheckpointActivity
{
    Question,
    Feedback
}

//implemented most of amber's old code, but instead of this file calling on reflectionUI to open the reflection, it calls on reflectionmanager
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{

    [Header("Checkpoint Identity")]
    public string checkpointId = "test_checkpoint_1";
    //identies one specific checkpoint

    [Header("Reflection Activity")]
    public CheckpointActivity activity = CheckpointActivity.Question;
    //determines whether or not we are giving feedback or questions
    [Range(1, 3)]
    public int cycleNumber = 1;


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
            if (activity == CheckpointActivity.Feedback)
            {
                reflectionManager.OnFeedbackCheckpointReached(checkpointId, cycleNumber);
            }
            else
            {
                reflectionManager.OnCheckpointReached(checkpointId, cycleNumber);//reflection manager now shows reflection
            }
        }
        //Reflectionmanager opens the reflectionUI
}
}


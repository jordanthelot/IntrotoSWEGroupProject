using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//changing the file to essentially let reflectionmanager choose the questions, quotes, reflections, and decide what’s next
//this files only job is to display content and send submissions to reflectionmanager

public class ReflectionUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject reflectionPanel;
    public TMP_Text reflectionQuestionText;
    public TMP_InputField reflectionInputField;
    public TMP_Text statusText; //loading/error message.
    public Button submitButton;
    [Header("Player")]
    public PlayerController playerController;

    public event Action<string> ResponseSubmitted;

    private bool isOpen;
    private bool isBusy;// set to true when the game is saving/loading something
    private float previousTimeScale;//restores the previous game speed
  

    private void Awake()
    {
        reflectionPanel.SetActive(false);
    }

    public void ShowReflection(string prompt, string savedAnswer = "")//added savedanswer to save players answer, just incase we want to allow users to repopen questions
    {
        if (!isOpen)
        {
            previousTimeScale = Time.timeScale;//making sure speed of game is preserved

            // Disable player movement while answering
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            // Pause gameplay
            isOpen = true;
            Time.timeScale = 0f;
        }

     
        reflectionQuestionText.text = prompt;
        reflectionInputField.text = savedAnswer;
        reflectionPanel.SetActive(true);

        if (statusText != null)
            statusText.text = "";

        reflectionInputField.Select();
        reflectionInputField.ActivateInputField();
    }
	public void ShowError(string message)
{
    if (statusText != null)
        statusText.text = message;

    Debug.LogWarning(message);
}


    public void ShowFeedback(CheckpointCycleVars cycle)//showing AI feedback, for now, we use dummy variables for testing
    {
        string prompt =
            cycle.AIResponse +
            "\n\n\"" + cycle.quoteText + "\"" +
            "\n— " + cycle.quoteAttribution +
            "\n\nWhat are your thoughts on this response and quote?";

        ShowReflection(prompt, cycle.playerReflectionText);
    }

    // Connect the Submit button's On Click to this method, bc nothing is currently attached there to my knowledge
    public void SubmitReflection()
    {
        if (!isOpen || isBusy)
            return;

        string response = reflectionInputField.text.Trim();

        if (string.IsNullOrEmpty(response))
        {
            ShowError("Please enter a response before continuing.");//changed to showerror to show the player the eror msg
            return;
        }

        ResponseSubmitted?.Invoke(response);

        //main change from amber's file- ReflectionUImanager does not handle restoring gameplay or resuming the game, reflection manager decides that
    }

    public void SetBusy(bool busy)
    {
        isBusy = busy;
        reflectionInputField.interactable = !busy;

        if (submitButton != null)
            submitButton.interactable = !busy;

        if (statusText != null)
            statusText.text = busy ? "Saving..." : "";
    }
    public void CloseReflection()//reflection manager will call this
{
    RestoreGameplay();
    reflectionPanel.SetActive(false);
}

private void RestoreGameplay()
{
    if (!isOpen)
        return;

    isOpen = false;
    Time.timeScale = previousTimeScale;

    if (playerController != null)
        playerController.enabled = true;
}
   

    private void OnDisable()
    {
        RestoreGameplay();
    }
}


using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// PBI 6 (Amber's tasks, written ahead): reflection-question UI and the player's answer field.
// Shows a question, freezes the player, and submits the answer through ReflectionSubmitter.
// Enter or the Submit button submits. If the answer is invalid, the reason is shown in red.
public class ReflectionQuestionUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_Text errorText;

    public static ReflectionQuestionUI Instance { get; private set; }
    public bool IsOpen => panel != null && panel.activeSelf;

    /// <summary>Fires after an answer is accepted and the panel closes.</summary>
    public event Action<ReflectionResponse> Answered;

    private string currentQuestionId;
    private string currentQuestion;
    private string currentCheckpointId;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        submitButton.onClick.AddListener(Submit);
        answerInput.onSubmit.AddListener(_ => Submit());
        answerInput.characterLimit = ResponseValidator.MaxLength;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(string questionId, string question, string checkpointId)
    {
        currentQuestionId = questionId;
        currentQuestion = question;
        currentCheckpointId = checkpointId;

        questionText.text = question;
        answerInput.text = "";
        errorText.text = "";
        panel.SetActive(true);
        PlayerController.InputLocked = true;

        answerInput.Select();
        answerInput.ActivateInputField();
    }

    public void Submit()
    {
        if (!IsOpen) return;

        if (ReflectionSubmitter.TrySubmit(currentQuestionId, currentQuestion, currentCheckpointId,
                                          answerInput.text, out ReflectionResponse response, out string error))
        {
            Hide();
            Answered?.Invoke(response);
        }
        else
        {
            errorText.text = error;
            answerInput.ActivateInputField();
        }
    }

    public void Hide()
    {
        panel.SetActive(false);

        // Typing an answer presses movement keys (A, D, Space). If a key-release is missed while
        // the text box has focus, that key stays "held" and the player walks on its own.
        // Resetting the keyboard clears any stuck keys before control returns to the player.
        if (Keyboard.current != null) InputSystem.ResetDevice(Keyboard.current);

        PlayerController.InputLocked = false;
    }
}


using UnityEngine;
using TMPro;

public class ReflectionUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject reflectionPanel;
    public TMP_Text reflectionQuestionText;
    public TMP_InputField reflectionInputField;
    [Header("Player")]
    public PlayerController playerController;
    private bool isReflectionOpen = false;

    void Start()
    {
        reflectionPanel.SetActive(false);
    }

    public void ShowReflection(string question)
    {
        if (isReflectionOpen)
            return;

        isReflectionOpen = true;
        // Disable player movement while answering
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        reflectionQuestionText.text = question;
        reflectionInputField.text = "";

        reflectionPanel.SetActive(true);

        // Pause gameplay
        Time.timeScale = 0f;

        reflectionInputField.Select();
        reflectionInputField.ActivateInputField();
    }

    public void SubmitReflection()
    {
        string response = reflectionInputField.text.Trim();

        if (string.IsNullOrEmpty(response))
        {
            Debug.Log("Please enter a response before continuing.");
            return;
        }

        Debug.Log("Reflection response: " + response);

        reflectionPanel.SetActive(false);
        isReflectionOpen = false;

        // Resume gameplay
        Time.timeScale = 1f;
        // Restore player movement
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    void OnDisable()
    {
        // Avoid leaving the game paused if this manager is disabled
        Time.timeScale = 1f;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isReflectionOpen)
        {
            ShowReflection(
                "How do you usually feel after spending a long time scrolling through social media?"
            );
        }
    }
}


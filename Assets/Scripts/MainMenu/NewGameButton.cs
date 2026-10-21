using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class NewGameButton : MonoBehaviour
{
    [SerializeField] private CheckpointCycle saveService;
    [SerializeField] private Button newGameButton;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private string startingScene = "GameScene";

    private bool isStarting;

    public async void OnNewGamePressed()
    {
        //general checks
        if (isStarting)
        {
            return;
        }
        if (saveService == null || !Application.CanStreamedLevelBeLoaded(startingScene))
        {
            ShowStatus(
                "Please provide a valid starting scene/ assign save service"
            );
            return;
        }
        isStarting = true;

        if (newGameButton != null)
        {
            newGameButton.interactable = false;

            try
            {
                ShowStatus("Starting a new game...");

                await saveService.StartNewGameAsync();

                Time.timeScale = 1f;

                SceneManager.LoadScene(startingScene);
            }
            catch (Exception x)
            {
                Debug.LogException(x);
                ShowStatus("Could not start new game");
            }
            finally
            {
                isStarting = false;
                if (newGameButton != null)
                {
                    newGameButton.interactable = true;
                }
            }
        }

    }
    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }
}

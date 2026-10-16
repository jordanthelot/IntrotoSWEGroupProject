
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterButtonAudio : MonoBehaviour
{
    public AudioClip buttonClickSound;

    public void SelectCapybara()
    {
        StartCoroutine(PlaySoundAndSelect("Capybara"));
    }

    public void SelectDuck()
    {
        StartCoroutine(PlaySoundAndSelect("Duck"));
    }

    public void SelectMonkey()
    {
        StartCoroutine(PlaySoundAndSelect("Monkey"));
    }

    private IEnumerator PlaySoundAndSelect(string character)
    {
        // Prevent repeated clicks while changing scenes
        UnityEngine.UI.Button[] buttons =
            FindObjectsByType<UnityEngine.UI.Button>(
                FindObjectsSortMode.None
            );

        foreach (UnityEngine.UI.Button button in buttons)
        {
            button.interactable = false;
        }

        // Play the sound before changing scenes
        if (buttonClickSound != null)
        {
            AudioSource.PlayClipAtPoint(
                buttonClickSound,
                Vector3.zero,
                0.6f
            );

            yield return new WaitForSecondsRealtime(0.2f);
        }

        // Remember the selected character
        CharacterSelectionManager.SelectedCharacter = character;

        Debug.Log("Selected character: " + character);

        // Open gameplay
        SceneManager.LoadScene("Gameplay");
    }
}

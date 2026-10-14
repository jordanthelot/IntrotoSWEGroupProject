
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    public static string SelectedCharacter = "Capybara";

    public void SelectCapybara()
    {
        SelectCharacter("Capybara");
    }

    public void SelectDuck()
    {
        SelectCharacter("Duck");
    }

    public void SelectMonkey()
    {
        SelectCharacter("Monkey");
    }

    private void SelectCharacter(string characterName)
    {
        SelectedCharacter = characterName;
        Debug.Log("Selected character: " + SelectedCharacter);

        SceneManager.LoadScene("Gameplay");
    }
}

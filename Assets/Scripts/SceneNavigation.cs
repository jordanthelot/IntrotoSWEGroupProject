
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void GoToLogin()
    {
        SceneManager.LoadScene("Login");
    }

    public void GoToCreateAccount()
    {
        SceneManager.LoadScene("CreateAccount");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToCharacterSelection()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    public void GoToGameplay()
    {
        SceneManager.LoadScene("Gameplay");
    }
}


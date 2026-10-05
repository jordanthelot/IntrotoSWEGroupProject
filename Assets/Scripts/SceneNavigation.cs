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
}

using UnityEngine;
using UnityEngine.SceneManagement;

// PBI 3: Connect menu options to game scenes.
// Put this on the SceneManager object in the MainMenu scene.
//
// Hook buttons in the Inspector: Button > On Click () > + > drag SceneManager object >
//   PlayButton              -> MainMenuController.Play
//   ReflectionHistoryButton -> MainMenuController.OpenReflectionHistory
//   AccountButton           -> MainMenuController.OpenAccount
// LogoutButton is left as Amber wired it (SceneNavigation.GoToLogin). Real logout
// (signing out and clearing the session) is PBI 14.
//
// Scene names are set in the Inspector, not hardcoded, because these scenes
// don't exist yet. Fill them in once the team creates them.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "";
    [SerializeField] private string reflectionHistorySceneName = "";
    [SerializeField] private string accountSceneName = "";

    public void Play() => Load(gameSceneName, "Game");
    public void OpenReflectionHistory() => Load(reflectionHistorySceneName, "Reflection History");
    public void OpenAccount() => Load(accountSceneName, "Account");

    private void Load(string sceneName, string label)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"MainMenuController: no scene name set for {label}. Set it in the Inspector.");
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"MainMenuController: scene '{sceneName}' is not in File > Build Profiles scene list.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}

using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

// PBI 2: Restore existing user session.
// Put this on the SceneManager object in the Login scene.
// If the player has a cached session from a previous run, they are signed back in
// and sent straight to the main menu. Otherwise they stay on the Login screen.
//
// Unity docs: if a cached session token exists, SignInAnonymouslyAsync() refreshes it
// regardless of whether the player signed in anonymously or through a platform provider
// (Username & Password counts as a provider). It is only called when SessionTokenExists
// is true, so it never creates a new anonymous account.
public class SessionRestorer : MonoBehaviour
{
    [Tooltip("Exact scene name to load after a session is restored. Must be in the Build Profiles scene list.")]
    [SerializeField] private string sceneAfterRestore = "MainMenu";

    [Tooltip("Seconds to wait for Unity Services to finish initializing.")]
    [SerializeField] private float initTimeoutSeconds = 10f;

    // Only try once per app launch. Amber's LogoutButton loads the Login scene directly;
    // without this flag, returning to Login while signed in would bounce straight back
    // to MainMenu.
    private static bool attemptedThisLaunch;

    private async void Start()
    {
        if (attemptedThisLaunch) return;
        attemptedThisLaunch = true;

        try
        {
            if (!await EnsureUnityServicesReady())
            {
                Debug.LogError("SessionRestorer: Unity Services did not initialize. Staying on Login.");
                return;
            }

            IAuthenticationService auth = AuthenticationService.Instance;

            if (auth.IsSignedIn)
            {
                Debug.Log($"SessionRestorer: already signed in as {auth.PlayerId}.");
                LoadNextScene();
                return;
            }

            if (!auth.SessionTokenExists)
            {
                Debug.Log("SessionRestorer: no cached session. Player must log in.");
                return;
            }

            await auth.SignInAnonymouslyAsync(); // restores the cached player only
            Debug.Log($"SessionRestorer: session restored for {auth.PlayerId}.");
            LoadNextScene();
        }
        catch (AuthenticationException ex)
        {
            // Token expired or invalid: clear it so the player gets a clean login.
            Debug.LogWarning($"SessionRestorer: could not restore session ({ex.ErrorCode}). Clearing token.");
            AuthenticationService.Instance.ClearSessionToken();
        }
        catch (RequestFailedException ex)
        {
            // Network or service error: keep the token; the player can still log in manually.
            Debug.LogWarning($"SessionRestorer: request failed ({ex.ErrorCode}): {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    // Resets the flag when Play mode starts, even with "Enter Play Mode Options" (no domain reload) on.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetLaunchFlag() => attemptedThisLaunch = false;

    // Jordan's UnityServicesInitializer starts initialization in Awake.
    // If it isn't in this scene, initialize here instead.
    private async Task<bool> EnsureUnityServicesReady()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        float waited = 0f;
        while (UnityServices.State != ServicesInitializationState.Initialized)
        {
            if (waited >= initTimeoutSeconds) return false;
            await Task.Delay(100);
            waited += 0.1f;
        }
        return true;
    }

    private void LoadNextScene()
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneAfterRestore))
        {
            Debug.LogError($"SessionRestorer: scene '{sceneAfterRestore}' is not in File > Build Profiles scene list.");
            return;
        }
        SceneManager.LoadScene(sceneAfterRestore);
    }
}

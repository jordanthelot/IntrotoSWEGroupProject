using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

// Connects Amber's Login / Create Account buttons to Jordan's UnityPlayerAccountLogin,
// and sends the player to the main menu once they are signed in.
//
// Put this on the SceneManager object in the Login and CreateAccount scenes
// (Mirror > Wire Login Buttons does it for you) and point the button's On Click at OnLoginPressed.
//
// Unity Player Accounts handles both new and existing accounts on Unity's sign-in page,
// so both buttons open the same page. Jordan's LogIn() is unchanged.
[RequireComponent(typeof(UnityPlayerAccountLogin))]
public class LoginFlow : MonoBehaviour
{
    [Tooltip("Exact scene name to load after sign-in. Must be in the Build Profiles scene list.")]
    [SerializeField] private string sceneAfterLogin = "MainMenu";

    [Tooltip("Seconds to wait for Unity Services to finish initializing.")]
    [SerializeField] private float initTimeoutSeconds = 10f;

    private UnityPlayerAccountLogin login;
    private bool listening;

    private void Awake()
    {
        login = GetComponent<UnityPlayerAccountLogin>();
    }

    // Button On Click target.
    public async void OnLoginPressed()
    {
        try
        {
            if (!await EnsureUnityServicesReady())
            {
                Debug.LogError("LoginFlow: Unity Services did not initialize. Try again.");
                return;
            }

            IAuthenticationService auth = AuthenticationService.Instance;
            if (auth.IsSignedIn)
            {
                LoadNextScene();
                return;
            }

            if (!listening)
            {
                auth.SignedIn += OnSignedIn;
                listening = true;
            }

            login.LogIn(); // Jordan's code: opens Unity's sign-in page in the browser
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void OnSignedIn()
    {
        if (this == null) return;
        Debug.Log($"LoginFlow: signed in as {AuthenticationService.Instance.PlayerId}. Loading {sceneAfterLogin}.");
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneAfterLogin))
        {
            Debug.LogError($"LoginFlow: scene '{sceneAfterLogin}' is not in File > Build Profiles scene list.");
            return;
        }
        SceneManager.LoadScene(sceneAfterLogin);
    }

    private async Task<bool> EnsureUnityServicesReady()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
            await UnityServices.InitializeAsync();

        float waited = 0f;
        while (UnityServices.State != ServicesInitializationState.Initialized)
        {
            if (waited >= initTimeoutSeconds) return false;
            await Task.Delay(100);
            waited += 0.1f;
        }
        return true;
    }

    private void OnDestroy()
    {
        if (listening && UnityServices.State == ServicesInitializationState.Initialized)
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
    }
}

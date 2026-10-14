using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

// PBI 1: Test successful / failed account creation (Unity Player Accounts).
//
// The team uses Unity Player Accounts (Jordan's UnityPlayerAccountLogin). Creating the
// Unity account itself happens on Unity's web page in the browser, so this test has two parts:
//
//   Automated, no browser:
//     1-2. Fake or empty Player Accounts tokens are rejected and no game account is created.
//   Hand step:
//     3.   Jordan's LogIn() opens the browser. Create a new account (or sign in) there.
//          The test waits up to `browserTimeoutSeconds` for you to finish.
//   Automated, after the hand step:
//     3.   The player is signed in to the game with a player ID.
//     4.   The game account was really created: signing out and signing back in with
//          CreateAccount = false (sign-in only, can't create) returns the same player ID.
//     5.   Pressing Login again while signed in does not create a second account.
//
// How to run: put this on an empty GameObject in the AuthTests scene and press Play.
// Read the PASS/FAIL lines and the summary in the Console.
// SignInWithUnityAsync API checked against com.unity.services.authentication 3.7.4.
[RequireComponent(typeof(UnityPlayerAccountLogin))]
public class AccountCreationTest : MonoBehaviour
{
    [Tooltip("How long to wait for you to finish the browser sign-up/sign-in.")]
    [SerializeField] private float browserTimeoutSeconds = 180f;

    [Tooltip("On: stay signed in at the end, so PBI 2 (session restore) can be tested next. Off: sign out.")]
    [SerializeField] private bool keepSignedInForPbi2 = true;

    private UnityPlayerAccountLogin login;
    private TestReport report;

    private async void Start()
    {
        // RequireComponent only applies when the component is first added, so objects
        // saved before the switch to Player Accounts may not have it yet.
        login = GetComponent<UnityPlayerAccountLogin>();
        if (login == null) login = gameObject.AddComponent<UnityPlayerAccountLogin>();
        report = new TestReport("AccountCreationTest");

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();

            IAuthenticationService auth = AuthenticationService.Instance;
            SignOutEverywhere();

            // ---- Expected failures: no game account may be created ----
            await ExpectRejected("Fake Player Accounts token is rejected", "not-a-real-token");
            await ExpectRejected("Empty Player Accounts token is rejected", "");

            // ---- Hand step: real account through the browser ----
            Debug.Log("AccountCreationTest: HAND STEP. A browser window is opening. " +
                      "Create a new Unity account (or sign in), then come back to Unity.");
            login.LogIn();

            float waited = 0f;
            while (!auth.IsSignedIn && waited < browserTimeoutSeconds)
            {
                await Task.Delay(500);
                waited += 0.5f;
                if (this == null) return; // Play mode was stopped
            }

            bool signedIn = auth.IsSignedIn && !string.IsNullOrEmpty(auth.PlayerId);
            report.Record("Account created and signed in through the browser", signedIn,
                signedIn ? "" : $"not signed in after {browserTimeoutSeconds:0}s");

            if (signedIn)
            {
                string playerId = auth.PlayerId;
                Debug.Log($"AccountCreationTest: game player ID {playerId}");

                // ---- The account exists: sign-in-only returns the same player ----
                auth.SignOut(); // keep the Player Accounts token
                await auth.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken,
                                                new SignInOptions { CreateAccount = false });
                report.Record("Account was saved (sign-in-only finds the same player)",
                    auth.IsSignedIn && auth.PlayerId == playerId,
                    $"got {(auth.IsSignedIn ? auth.PlayerId : "not signed in")}");

                // ---- Pressing Login again doesn't create a duplicate ----
                login.LogIn(); // Jordan's code should refuse: already signed in
                await Task.Delay(1000);
                report.Record("Login while signed in keeps the same account",
                    auth.IsSignedIn && auth.PlayerId == playerId,
                    $"got {(auth.IsSignedIn ? auth.PlayerId : "not signed in")}");

                if (!keepSignedInForPbi2)
                {
                    login.LogOut();
                    Debug.Log("AccountCreationTest: signed out.");
                }
                else
                {
                    Debug.Log("AccountCreationTest: still signed in, ready for the PBI 2 session-restore test.");
                }
            }
        }
        catch (Exception ex)
        {
            report.Record("Test run finished without an unexpected error", false, ex.Message);
            Debug.LogException(ex);
        }

        report.Summary();
    }

    // Signing in with a bad token must throw and must leave no signed-in player.
    private async Task ExpectRejected(string caseName, string token)
    {
        IAuthenticationService auth = AuthenticationService.Instance;
        bool threw = false;
        try
        {
            await auth.SignInWithUnityAsync(token);
        }
        catch (RequestFailedException)
        {
            threw = true;
        }
        catch (ArgumentException)
        {
            threw = true;
        }

        bool ok = threw && !auth.IsSignedIn;
        report.Record(caseName, ok,
            $"threw={threw}, signedIn={auth.IsSignedIn}");
        SignOutEverywhere();
    }

    private static void SignOutEverywhere()
    {
        IAuthenticationService auth = AuthenticationService.Instance;
        if (auth.IsSignedIn) auth.SignOut(true);
        if (auth.SessionTokenExists) auth.ClearSessionToken();
        if (PlayerAccountService.Instance.IsSignedIn) PlayerAccountService.Instance.SignOut();
    }
}

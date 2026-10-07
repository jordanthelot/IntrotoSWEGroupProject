using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

// PBI 1: Test successful / failed account creation.
// Runs Jordan's UsernamePasswordAuth.SignUpWithUsernamePasswordAsync against the real
// Unity Authentication service and reports PASS/FAIL for each case in the Console.
//
// How to run: put this on an empty GameObject in a scene by itself (e.g. AuthTests),
// press Play, and read the Console. UsernamePasswordAuth is added automatically.
//
// Jordan's methods catch their own exceptions and return nothing, so each case checks
// AuthenticationService.Instance.IsSignedIn afterwards: signed in = sign-up succeeded.
// Red exception logs from UsernamePasswordAuth during the "should fail" cases are expected.
//
// Rules tested (Unity docs, Username & Password provider):
//   Username: 3-20 chars; letters, numbers, and . - @ _ only.
//   Password: 8-30 chars; at least 1 lowercase, 1 uppercase, 1 number, 1 symbol.
[RequireComponent(typeof(UsernamePasswordAuth))]
public class AccountCreationTest : MonoBehaviour
{
    [Tooltip("Off: the test account is deleted at the end. On: it stays signed in, so you can test PBI 2 (session restore) next.")]
    [SerializeField] private bool keepTestAccountSignedIn = false;

    private const string ValidPassword = "Wellness#2026";

    private UsernamePasswordAuth auth;
    private readonly List<string> results = new List<string>();
    private int passed, failed;

    private async void Start()
    {
        auth = GetComponent<UsernamePasswordAuth>();

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();

            ResetAuthState();

            // Unique per run, so the "new account" case never collides with a previous run.
            string username = "test_" + DateTime.UtcNow.ToString("MMddHHmmss"); // 15 chars

            // ---- Expected failures (account must NOT be created) ----
            await ExpectSignUp("Username too short (2 chars)", "ab", ValidPassword, false);
            await ExpectSignUp("Username has invalid characters", "bad name!", ValidPassword, false);
            await ExpectSignUp("Username too long (21 chars)", new string('a', 21), ValidPassword, false);
            await ExpectSignUp("Password too short (7 chars)", username + "x", "Ab1#xyz", false);
            await ExpectSignUp("Password missing uppercase", username + "x", "wellness#2026", false);
            await ExpectSignUp("Password missing number", username + "x", "Wellness#abcd", false);
            await ExpectSignUp("Password missing symbol", username + "x", "Wellness2026", false);

            // ---- Expected success ----
            await ExpectSignUp("Valid new account", username, ValidPassword, true);

            // ---- Duplicate username must fail ----
            // Unity's docs don't state the exact error for a taken username; a PASS here
            // means the service refused to create a second account with that name.
            ResetAuthState();
            await ExpectSignUp("Duplicate username", username, ValidPassword, false);

            // ---- The created account works with Jordan's sign-in ----
            ResetAuthState();
            await auth.SignInWithUsernamePasswordAsync(username, ValidPassword);
            Record("Sign in to the new account", AuthenticationService.Instance.IsSignedIn);

            // ---- Cleanup ----
            if (AuthenticationService.Instance.IsSignedIn && !keepTestAccountSignedIn)
            {
                await AuthenticationService.Instance.DeleteAccountAsync();
                Debug.Log($"AccountCreationTest: deleted test account '{username}'.");
            }
            else if (keepTestAccountSignedIn)
            {
                Debug.Log($"AccountCreationTest: kept '{username}' / {ValidPassword} signed in for the PBI 2 test.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("AccountCreationTest: test run stopped early.");
            Debug.LogException(ex);
        }

        string summary = $"AccountCreationTest: {passed} passed, {failed} failed\n" + string.Join("\n", results);
        if (failed == 0) Debug.Log(summary); else Debug.LogError(summary);
    }

    private async Task ExpectSignUp(string caseName, string username, string password, bool shouldSucceed)
    {
        ResetAuthState();
        await auth.SignUpWithUsernamePasswordAsync(username, password);
        bool succeeded = AuthenticationService.Instance.IsSignedIn;
        Record(caseName, succeeded == shouldSucceed,
            $"expected {(shouldSucceed ? "success" : "failure")}, got {(succeeded ? "success" : "failure")}");
    }

    // Jordan's methods refuse to run while signed in, so start every case signed out
    // with no cached session.
    private static void ResetAuthState()
    {
        IAuthenticationService a = AuthenticationService.Instance;
        if (a.IsSignedIn) a.SignOut();
        if (a.SessionTokenExists) a.ClearSessionToken();
    }

    private void Record(string caseName, bool ok, string detail = "")
    {
        if (ok) passed++; else failed++;
        string line = $"{(ok ? "PASS" : "FAIL")}  {caseName}{(ok || detail == "" ? "" : "  (" + detail + ")")}";
        results.Add(line);
        if (ok) Debug.Log("AccountCreationTest: " + line); else Debug.LogError("AccountCreationTest: " + line);
    }
}

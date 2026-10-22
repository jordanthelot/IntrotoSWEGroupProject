using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System;
using Unity.Services.Authentication.PlayerAccounts;

public class UnityPlayerAccountLogin : MonoBehaviour
{
    private IPlayerAccountService playerAccounts;
    private bool loginInprogress;
    private bool completegamelogin;

    public async void LogIn()
    {   //handling errors that could occur from user pressing login button
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("Unity Services are not ready");
            return;
        }
        if (loginInprogress)
        {
            Debug.Log("A login attempt is already in progress.");
            return;
        }
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("A player is already signed in");
            return;
        }

        //now we get to the case where the user is not signed in and needs to log in
        if (playerAccounts == null)
        {
            playerAccounts = PlayerAccountService.Instance;
            playerAccounts.SignedIn += OnPlayerAccountSignedIn;
            playerAccounts.SignInFailed += OnPlayerAccountSignInFailed;

        }

        loginInprogress = true;

        try
        {
            if (playerAccounts.IsSignedIn)
            {
                //event that player is already signed in
                OnPlayerAccountSignedIn();
            }
            else
            {
                Debug.Log("Opening the Unity Login page");
                await playerAccounts.StartSignInAsync();
            }
        }
        catch (Exception error)
        {
            loginInprogress = false;
            Debug.LogError("Could not start login");
            Debug.LogException(error);
        }
    }
    private async void OnPlayerAccountSignedIn()
    {
        
        if (this == null || completegamelogin)
        {
            return;
        }

        completegamelogin = true;

        try
        {
            // Signs a player into Unity game  by sending their authorization token to Unity's Authentication service.
            await AuthenticationService.Instance.SignInWithUnityAsync(playerAccounts.AccessToken);

            if (this == null)
            {
                return;
            }


            //Note for later; Can load the player's cloud save data here as well
        }
        catch (Exception error)
        {
            Debug.LogError("Could not sign the player into the game.");
            Debug.LogException(error);
        }
        finally
        {
            completegamelogin = false;
            loginInprogress = false;
        }
    }

    //handling the event that the user closes the browser while attempting to login
    public void RetryBroswerLogin()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("Unity Services are not ready");
            return;
        }

        if (completegamelogin)
        {
            Debug.Log("Game sign-in is finishing. Please wait");
            return;
        }

        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("You are already signed in");
            return;
        }

        loginInprogress = false;
        Debug.Log("Restarting broswer login. Use newly opened tab");
        LogIn();


    }

    private void OnPlayerAccountSignInFailed(RequestFailedException error)
    {
        loginInprogress = false;
        Debug.LogError($"Broswer login failed: {error.Message}");
    }

    public void LogOut()
    {
        //once again, making sure unity services are initialized before starting anything
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            return;
        }
        if (loginInprogress)
        {
            Debug.LogWarning("Wait for the login attempt to finish");
            return;
        }

        AuthenticationService.Instance.SignOut(true);
        PlayerAccountService.Instance.SignOut();

        Debug.Log("Player signed out");
    }

    private void OnDestroy()
    {
        if (playerAccounts == null)
        {
            return;
        }

        playerAccounts.SignedIn -= OnPlayerAccountSignedIn;
        playerAccounts.SignInFailed -= OnPlayerAccountSignInFailed;
    }
}

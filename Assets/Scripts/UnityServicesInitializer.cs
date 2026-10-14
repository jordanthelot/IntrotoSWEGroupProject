
using System;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Authentication;
//Initiliazes Unity's Authentication and Cloud save SDK

public class UnityServicesInitializer : MonoBehaviour
{
    private IAuthenticationService auth;
    //holds a references to the authentication service

    private async void Awake()
    {
        try
        {
            //Initialization the Authentication/cloud save services actually begin
            Debug.Log("Initializing Unity Services");
            await UnityServices.InitializeAsync();
            SetupEvents();
            Debug.Log("Unity Services Initialized. Authorization ready to begin");


        }
        catch (Exception e)
        {
            Debug.Log("Initialization failed");
            Debug.LogException(e);
        }
    }
    private void SetupEvents()
    //Autentication events
    {
        auth = AuthenticationService.Instance;
        //Instead of creating a new authentication object every time, instance remembers the user

        auth.SignedIn += OnSignedIn;
        auth.SignInFailed += OnSignInFailed;
        auth.SignedOut += OnSignedOut;
        auth.Expired += OnSessionExpired;
    }

    private void OnSignedIn()
    {
        Debug.Log($"Player signed in. Player ID: {auth.PlayerId}");
    }
    private void OnSignInFailed(RequestFailedException error)
    {
        Debug.LogError($"Sign-in failed: {error.Message}");
    }
    private void OnSignedOut()
    {
        Debug.Log("Player signed out.");
    }
    private void OnSessionExpired()
    {
        Debug.LogWarning("Session expired. Player needs to sign in again.");
    }

    private void OnDestroy()
    {
        if (auth == null)
        {
            return;
        }
        auth.SignedIn -= OnSignedIn;
        auth.SignInFailed -= OnSignInFailed;
        auth.SignedOut -= OnSignedOut;
        auth.Expired -= OnSessionExpired;
        //function prevents memory leaks whenever a scene or game object is destroyed; useful for testing
    }
}


using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System.Threading.Tasks;
public class UsernamePasswordAuth : MonoBehaviour
{
    //these functions have to eventually be referenced in our login window script
    public async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("Unity Services are not ready yet.");
            return;
        }//safeguard added in every function so that log-in is not given before unity services are initialized
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Sign out before signing into another account.");
            return;
        }
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("Unity Services are not ready yet.");
            return;
        }
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Sign out before signing into another account.");
            return;
        }
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public async Task UpdatePasswordAsync(string currentPassword, string newPassword)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.LogWarning("Unity Services are not ready yet.");
            return;
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Sign in before changing your password.");
            return;
        }
        try
        {
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
            Debug.Log("Password updated.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }





}

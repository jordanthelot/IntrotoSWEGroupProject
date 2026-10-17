using UnityEngine;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System.Threading.Tasks; //gives us access to the async and await keywords
using System.Collections.Generic; //allows us to use high-performance data structures like queues and
using System;


//this file handles the event that for some reason, a user session ends at the middle of a checkpoint, and we need to save their reflection status and load it for them when they come back
public class CheckpointCycle : MonoBehaviour
{
    private static void CheckReady()
    {//simply seeing if unity's services are initialized, like authentication
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            throw new InvalidOperationException("Unity Services must be initialized");
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            throw new InvalidOperationException("Player must sign in before trying to save");
        }
    }

    //helper function to help us identify the reflecitons, utilizing a dictionary that will be defined later
    private static string GetReflectionKey(string cycleId)
{
    if (string.IsNullOrWhiteSpace(cycleId))
    {
        throw new ArgumentException(
            "A cycleId is required.", nameof(cycleId));
    }

    return $"reflectionCycle_{cycleId}";
}

    //saving the reflection data at the specific checkpoint for the player
    //the whole point of reflectionmanager.cs is to define what goes in reflectionJson, in other words, what are actually saving from the reflections?
    public async Task SaveReflectionAsync(string cycleId, string reflectionJson)
    {
        CheckReady();

        if (string.IsNullOrWhiteSpace(reflectionJson))
        {
            throw new ArgumentException("Reflection data is required.", nameof(reflectionJson));
        }
        string key = GetReflectionKey(cycleId);

        var data = new Dictionary<string, object>
        {
            {key, reflectionJson}
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    public async Task<string> LoadReflectionAsync(string cycleID)
    {
        CheckReady();

        string key = GetReflectionKey(cycleID);
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

        if (data.TryGetValue(key, out var savedReflection))
        {
            return savedReflection.Value.GetAs<string>();
        }

        return null;
    }
}

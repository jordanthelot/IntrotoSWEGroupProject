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

    private const string PlayerProgressKey = "reflection_player_progress";

    public async Task<ReflectionPlayerProgress> LoadPlayerProgressAsync()// Call back to our class in  CheckpointCycleVars
    {
        CheckReady();

        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { PlayerProgressKey });

        if (!data.TryGetValue(PlayerProgressKey, out var savedProgress))//data does not exist for this player
        {
            return new ReflectionPlayerProgress();
        }

        string json = savedProgress.Value.GetAs<string>();
        ReflectionPlayerProgress progress = JsonUtility.FromJson<ReflectionPlayerProgress>(json);

        if (progress == null || progress.activeCycleNumber < 1 || progress.answeredCheckpointIds == null)
        {
            throw new InvalidOperationException("Player has no saved progress");
        }

        return progress;
    }

    public async Task SaveCycleAndPlayerProgressAsync(CheckpointCycleVars cycle, ReflectionPlayerProgress progress)
    {
        CheckReady();

        if (cycle == null || progress == null)
        {
            throw new ArgumentException("Need data to save");
        }

        string cycleKey = GetReflectionKey(cycle.cycleId, progress.playthroughId);

        var data = new Dictionary<string, object>//data saved on unity cloud as key value pairs
        {
            {cycleKey, JsonUtility.ToJson(cycle)}, {PlayerProgressKey, JsonUtility.ToJson(progress)}
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    //helper function to help us identify the reflecitons, utilizing a dictionary that will be defined later
    private static string GetReflectionKey(string cycleId, string playthroughId = "")
    {
        if (string.IsNullOrWhiteSpace(cycleId))
        {
            throw new ArgumentException(
                "A cycleId is required.", nameof(cycleId));
        }

        if (string.IsNullOrWhiteSpace(playthroughId))
        {
            return $"reflectionCycle_{cycleId}";
        }

        return $"reflectionCycle_{playthroughId}_{cycleId}";
    }
    //this function below preserves previous game progress

    public async Task<ReflectionPlayerProgress> StartNewGameAsync()
    {
        CheckReady();

        ReflectionPlayerProgress newProgress = new ReflectionPlayerProgress
        {
            usedQuestionIds = new List<string>(),
            playthroughId = Guid.NewGuid().ToString("N"),//generates a unique 32 character id, commonly used for session storage
            activeCycleNumber = 1,
            answeredCheckpointIds = new List<string>()
        };

        CheckpointCycleVars firstCycle = new CheckpointCycleVars
        {
            cycleId = "cycle_01",
            stage = ReflectionStage.CollectingAnswers
        };

        await SaveCycleAndPlayerProgressAsync(firstCycle, newProgress);

        return newProgress;

    }

    //saving the reflection data at the specific checkpoint for the player
    public async Task SaveReflectionAsync(string cycleId, string reflectionJson, string playthroughId = "")
    {
        CheckReady();

        if (string.IsNullOrWhiteSpace(reflectionJson))
        {
            throw new ArgumentException("Reflection data is required.", nameof(reflectionJson));
        }
        string key = GetReflectionKey(cycleId, playthroughId);

        var data = new Dictionary<string, object>
        {
            {key, reflectionJson}
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    public async Task<string> LoadReflectionAsync(string cycleID, string playthroughId = "")
    {
        CheckReady();

        string key = GetReflectionKey(cycleID, playthroughId);
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

        if (data.TryGetValue(key, out var savedReflection))
        {
            return savedReflection.Value.GetAs<string>();
        }

        return null;
    }


}

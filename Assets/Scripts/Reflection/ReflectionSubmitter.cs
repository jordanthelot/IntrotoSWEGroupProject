using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// Where submitted responses get saved. PBI 7 (Jordan/Raul: "Save responses to database")
// implements this, then sets ReflectionSubmitter.Store = new TheirStore(); at startup.
public interface IReflectionResponseStore
{
    Task SaveAsync(ReflectionResponse response);
}

// PBI 6: Implement response submission.
// Validates the answer, stamps it with the time and checkpoint, keeps it for this session,
// announces it (Submitted event), and hands it to the database store once PBI 7 provides one.
public static class ReflectionSubmitter
{
    public static IReflectionResponseStore Store { get; set; }

    /// <summary>Fires after every accepted answer.</summary>
    public static event Action<ReflectionResponse> Submitted;

    private static readonly List<ReflectionResponse> sessionResponses = new List<ReflectionResponse>();
    public static IReadOnlyList<ReflectionResponse> SessionResponses => sessionResponses;

    public static bool TrySubmit(string questionId, string questionText, string checkpointId, string rawAnswer,
                                 out ReflectionResponse response, out string error)
    {
        response = null;

        if (string.IsNullOrWhiteSpace(questionId))
        {
            error = "Missing question. Please try again.";
            return false;
        }

        if (!ResponseValidator.Validate(rawAnswer, out string cleaned, out error))
            return false;

        response = new ReflectionResponse
        {
            questionId = questionId,
            questionText = questionText,
            answer = cleaned,
            checkpointId = checkpointId,
            timestampUtc = DateTime.UtcNow.ToString("o"),
        };

        sessionResponses.Add(response);
        Submitted?.Invoke(response);

        if (Store != null) _ = SaveSafely(response);
        else Debug.Log($"Reflection submitted for '{questionId}' at {checkpointId}. Not stored yet: database saving is PBI 7.");

        return true;
    }

    private static async Task SaveSafely(ReflectionResponse response)
    {
        try { await Store.SaveAsync(response); }
        catch (Exception ex) { Debug.LogException(ex); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        sessionResponses.Clear();
        Submitted = null;
    }
}

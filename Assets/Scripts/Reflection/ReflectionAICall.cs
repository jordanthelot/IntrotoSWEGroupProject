using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Unity.Services.Authentication;
using UnityEngine.Networking;
//I changed Raul's logic for returning the ML program variables
[Serializable]
public class ReflectionFeedbackData
{
    public string AIresponse;
    public string quoteId;
    public string quoteText;
    public string quotefrom;
    //What the ML algorithm read from the player's three answers (overall + per answer)
}

public class ReflectionAICall : MonoBehaviour
{
    [Header("Feedback Backend")]
    [SerializeField]
    private string feedbackUrl = "https://the-mirror-feedback.onrender.com/";

    [Serializable]
    private class QuestionAnswerPayload
    {
        public string questionId;
        public string questionText;
        public string answerText;
    }
    [Serializable]
    private class FeedbackRequestPayload
    {
        public List<QuestionAnswerPayload> questions = new List<QuestionAnswerPayload>();
    }
    //Used when the model can't run, so the player gets safe (neutral, supportive) content


    public async Task<ReflectionFeedbackData> GenerateFeedbackAsync(
        List<CheckpointQuestionData> questions
    )
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            throw new InvalidOperationException("Sign in before requesting feedback");
        }
        if (questions == null || questions.Count != 3)
        {
            throw new InvalidOperationException("AI response needs 3 questions.");
        }
        FeedbackRequestPayload payload = new FeedbackRequestPayload();

        foreach (CheckpointQuestionData question in questions)
        {
            if (question == null || question.status != QuestionStatus.Answered || string.IsNullOrWhiteSpace(question.questionId) || string.IsNullOrWhiteSpace(question.questionText) || string.IsNullOrWhiteSpace(question.answerText))
            {
                throw new InvalidOperationException("All questions must be answered before feedback.");
            }
            payload.questions.Add(new QuestionAnswerPayload
            {
                questionId = question.questionId,
                questionText = question.questionText,
                answerText = question.answerText
            });
        }
        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(
            feedbackUrl,
            UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + AuthenticationService.Instance.AccessToken
            );
            request.SetRequestHeader(
                "Content-Type", "application/json");
            request.SetRequestHeader(
                "Accept", "application/json");

            request.timeout = 180;

            await Awaitable.FromAsyncOperation(
                request.SendWebRequest());

            if (request.result != UnityWebRequest.Result.Success)
{
    throw new InvalidOperationException(
        $"Feedback request failed " +
        $"(HTTP {request.responseCode}): " +
        request.downloadHandler.text);
}

            ReflectionFeedbackData feedback =
                JsonUtility.FromJson<ReflectionFeedbackData>(
                    request.downloadHandler.text);

            if (feedback == null ||
                string.IsNullOrWhiteSpace(feedback.AIresponse) ||
                string.IsNullOrWhiteSpace(feedback.quoteId) ||
                string.IsNullOrWhiteSpace(feedback.quoteText) ||
                string.IsNullOrWhiteSpace(feedback.quotefrom))
            {
                throw new InvalidOperationException(
                    "The backend returned incomplete feedback.");
            }

            Debug.Log("Personalized feedback received.");

            return feedback;
        }
    }
}

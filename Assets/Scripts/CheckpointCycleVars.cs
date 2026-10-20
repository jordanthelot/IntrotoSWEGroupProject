using System;
using System.Collections.Generic;

//This file is dedicated to declaring the variables that will be used in our other file, reflectionmanager.cs
//What defines a reflection in progress, being started, finished, or skipping a question? Reflectionmanager.cs will handle those situations using these variables

public enum QuestionStatus
{
    NotStarted,
    InProgress,
    Answered,
    Skipped
}

[Serializable]
public class ReflectionPlayerProgress//keeping track of how many reflections player has completed
{
    public int activeCycleNumber = 1;
    public List<string> answeredCheckpointIds = new List<string>();
}

public enum ReflectionStage
{
    CollectingAnswers,
    AwaitingFeedback,//from AI
    AwaitingPlayerReflection,
    Completed

}

[Serializable]
public class CheckpointQuestionData//reflects one question and it's response
{
    public string checkpointId;

    public string questionText;
    public string questionId;
    public string answerText = "";
    public QuestionStatus status = QuestionStatus.NotStarted;
    //UTC timestamps in ISO 8601 ("o") format; empty until the moment happens
    public string shownAtUtc = "";
    public string answeredAtUtc = "";
}

[Serializable]
public class CheckpointCycleVars//represents an entire reflection and it's process
{
    public string cycleId;
    public string checkpointId;
    public QuestionStatus status = QuestionStatus.NotStarted;
    public ReflectionStage stage = ReflectionStage.CollectingAnswers;

    public List<CheckpointQuestionData> questions = new List<CheckpointQuestionData>();
    // During question collection: 0, 1, or 2.
    // After all three answers: 3; do not use it to index the list.
    public int currentQuestionIndex = 0;
    public string AIResponse = "";
    public string quoteId = "";
    public string quoteText = "";
    public string quoteAttribution = "";
    //players response to the ai's response
    public string playerReflectionText = "";
    //UTC timestamp (ISO 8601) of when the player submitted their final reflection
    public string completedAtUtc = "";
}
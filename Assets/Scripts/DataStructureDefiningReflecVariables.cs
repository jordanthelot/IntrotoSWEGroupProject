using System;
using System.Collections.Generic;

//This file is dedicated to declaring the variables that will be used in our other file, reflectionmanager.cs
//What defines a reflection in progress, being started, finished, or skipping a question? Reflectionmanager.cs will handle those situations using these variables
public enum ReflectionStatus
{
    NotStarted,
    InProgress,
    Completed
}

public enum QuestionStatus
{
    NotStarted,
    InProgress,
    Answered,
    Skipped
}

[Serializable]
public class ReflectionQuestionProgress//reflects one question and it's response
{
    public string questionId;
    public string answerText = "";
    public QuestionStatus status = QuestionStatus.NotStarted;
}

[Serializable]
public class ReflectionProgress//represents an entire reflection
{
    public string checkpointId;
    public ReflectionStatus status = ReflectionStatus.NotStarted;

    public List<ReflectionQuestionProgress> questions = new List<ReflectionQuestionProgress>();
    public int currentQuestionIndex = 0;
    public string companionResponse = "";
}
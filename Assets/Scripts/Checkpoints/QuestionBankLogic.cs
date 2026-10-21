using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]//file dedicated to reading our question bank file, and returning 
public class QuestionDefinition
{
    public string questionId;
    public string questionText;
    public List<string> topicTags;
    public bool isOpeningQuestion;
}

[Serializable]
public class QuestionBankData
{
    public List<QuestionDefinition> questions;
}

public class QuestionBankLogic : MonoBehaviour
{
    [SerializeField] private TextAsset questionBankJson;//where the questionbank file will be attached

    private List<QuestionDefinition> questions;

    private void Awake()
    {
        try
        {
            LoadQuestions();
        }
        catch (Exception ex)
        {
            Debug.LogError("Could not load questions: " + ex.Message);
        }
    }
    private void LoadQuestions()
    {
        if (questions != null)
        {
            return;
        }

        if (questionBankJson == null)
        {
            throw new InvalidOperationException("Assign Questionbank.json");
        }
        QuestionBankData bank = JsonUtility.FromJson<QuestionBankData>(questionBankJson.text);
        if (bank == null || bank.questions == null || bank.questions.Count == 0)
        {
            throw new InvalidOperationException("Question bank is not formatted correctly");
        }

        HashSet<string> questionIds = new HashSet<string>();//Used for Id checks

        foreach (QuestionDefinition question in bank.questions)
        {
            if (question == null || string.IsNullOrWhiteSpace(question.questionId) || string.IsNullOrWhiteSpace(question.questionText) || question.topicTags == null)
            {
                throw new InvalidOperationException("A question has missing data");
            }
            if (!questionIds.Add(question.questionId))
            {
                throw new InvalidOperationException("Duplicate question ID: " + question.questionId);
            }
        }
        questions = bank.questions;

        Debug.Log($"Question bank loaded: {questions.Count} questions");
    }
    public QuestionDefinition GetQuestionbyId(string questionId)
    {
        LoadQuestions();

        return questions.Find(question => question.questionId == questionId);//simply retrieving the question id from the json file
    }

    public QuestionDefinition GetRandomQuestion(List<string> usedQuestionIds, bool opening = false, string topic = null)
    {
        LoadQuestions();

        List<QuestionDefinition> eligibleQuestions = new List<QuestionDefinition>();

        foreach (QuestionDefinition question in questions)
        {
            if (usedQuestionIds != null && usedQuestionIds.Contains(question.questionId))
            {
                continue;
            }

            if (opening && !question.isOpeningQuestion)
            {
                continue;
            }
            if (!string.IsNullOrWhiteSpace(topic) && !question.topicTags.Contains(topic))
            {
                continue;
            }

            eligibleQuestions.Add(question);
        }
        if (eligibleQuestions.Count == 0)
        {
            return null;

        }
        int index = UnityEngine.Random.Range(0, eligibleQuestions.Count);

        return eligibleQuestions[index];
    }


}



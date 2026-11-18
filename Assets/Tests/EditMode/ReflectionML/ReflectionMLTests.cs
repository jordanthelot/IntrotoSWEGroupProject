using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

//Checks the C# ML pipeline against the Python results in ml_fixtures.json (made by ML/export_unity_assets.py).
//Run in Unity: Window > General > Test Runner > EditMode.
public class ReflectionMLTests
{
    [Serializable]
    private class FixtureAnswer
    {
        public string questionId;
        public string kind;
        public string text;
        public List<int> tokenIds;
        public List<float> scores;
        public string expectedValence;
    }

    [Serializable]
    private class TokenizerCase
    {
        public string text;
        public List<int> tokenIds;
    }

    [Serializable]
    private class FixtureCombo
    {
        public string code;
        public List<string> kinds;
        public List<string> answerValences;
        public string expectedOverall;
    }

    [Serializable]
    private class Fixtures
    {
        public string groupScoring;
        public List<string> labels;
        public List<FixtureAnswer> answers;
        public List<TokenizerCase> tokenizerCases;
        public List<FixtureCombo> combos;
    }

    //Returns Python's scores for whichever answer the token ids belong to, so the logic can be tested without the model
    private class FixtureScorer : IEmotionScorer
    {
        private readonly Dictionary<string, float[]> scoresByIds = new Dictionary<string, float[]>();

        public FixtureScorer(IEnumerable<FixtureAnswer> answers)
        {
            foreach (FixtureAnswer answer in answers)
            {
                scoresByIds[string.Join(",", answer.tokenIds)] = answer.scores.ToArray();
            }
        }

        public float[] Score(int[] tokenIds)
        {
            return scoresByIds[string.Join(",", tokenIds)];
        }
    }

    private static readonly string[] ControlQuestionIds = { "q022", "q013", "q031" };
    private const float ScoreTolerance = 0.001f;

    private Fixtures fixtures;
    private RobertaTokenizer tokenizer;
    private MLAlgorithm realAlgorithm;
    private InferenceEngineEmotionScorer realScorer;

    [OneTimeSetUp]
    public void LoadFixtures()
    {
        string path = Path.Combine(Application.dataPath, "Tests", "EditMode", "ReflectionML", "ml_fixtures.json");
        fixtures = JsonUtility.FromJson<Fixtures>(File.ReadAllText(path));
        tokenizer = RobertaTokenizer.FromText(
            Resources.Load<TextAsset>(ReflectionMLLoader.ResourceFolder + "goemotions_vocab").text,
            Resources.Load<TextAsset>(ReflectionMLLoader.ResourceFolder + "goemotions_merges").text);
    }

    [OneTimeTearDown]
    public void DisposeModel()
    {
        realScorer?.Dispose();
    }

    private MLAlgorithm RealAlgorithm()
    {
        if (realAlgorithm == null)
        {
            realAlgorithm = ReflectionMLLoader.Load(out realScorer);
        }
        return realAlgorithm;
    }

    private string AnswerText(string questionId, string kind)
    {
        return fixtures.answers.Find(answer => answer.questionId == questionId && answer.kind == kind).text;
    }

    [Test]
    public void Tokenizer_MatchesPythonTokenIds()
    {
        List<TokenizerCase> cases = new List<TokenizerCase>(fixtures.tokenizerCases);
        foreach (FixtureAnswer answer in fixtures.answers)
        {
            cases.Add(new TokenizerCase { text = answer.text, tokenIds = answer.tokenIds });
        }
        foreach (TokenizerCase testCase in cases)
        {
            CollectionAssert.AreEqual(testCase.tokenIds, tokenizer.Encode(testCase.text), $"Text: \"{testCase.text}\"");
        }
    }

    [Test]
    public void Logic_WithPythonScores_MatchesPythonValences()
    {
        MLAlgorithm algorithm = new MLAlgorithm(tokenizer, new FixtureScorer(fixtures.answers), fixtures.labels);
        foreach (FixtureAnswer answer in fixtures.answers)
        {
            Assert.AreEqual(answer.expectedValence, algorithm.ScoreAnswer(answer.text).valence,
                            $"{answer.questionId} {answer.kind}: \"{answer.text}\"");
        }
        foreach (FixtureCombo combo in fixtures.combos)
        {
            List<string> texts = new List<string>();
            for (int i = 0; i < 3; i++) texts.Add(AnswerText(ControlQuestionIds[i], combo.kinds[i]));
            Assert.AreEqual(combo.expectedOverall, algorithm.ClassifyAnswers(texts).overallValence, $"Combo {combo.code}");
        }
    }

    [Test]
    public void Model_InUnity_MatchesPythonScores()
    {
        MLAlgorithm algorithm = RealAlgorithm();
        foreach (FixtureAnswer answer in fixtures.answers)
        {
            float[] scores = algorithm.ScoreText(answer.text);
            for (int i = 0; i < scores.Length; i++)
            {
                Assert.AreEqual(answer.scores[i], scores[i], ScoreTolerance,
                                $"{answer.questionId} {answer.kind}, label {fixtures.labels[i]}");
            }
        }
    }

    [Test]
    public void Model_InUnity_AllCombinationsMatchPython()
    {
        MLAlgorithm algorithm = RealAlgorithm();
        foreach (FixtureCombo combo in fixtures.combos)
        {
            List<string> texts = new List<string>();
            for (int i = 0; i < 3; i++) texts.Add(AnswerText(ControlQuestionIds[i], combo.kinds[i]));
            ReflectionEmotionResult result = algorithm.ClassifyAnswers(texts);
            Assert.AreEqual(combo.expectedOverall, result.overallValence, $"Combo {combo.code}");
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(combo.answerValences[i], result.answers[i].valence, $"Combo {combo.code}, answer {i + 1}");
            }
        }
    }
}

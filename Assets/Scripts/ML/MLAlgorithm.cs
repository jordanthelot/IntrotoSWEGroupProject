using System;
using System.Collections.Generic;

//Anything that turns token ids into one score (0..1) per emotion label.
//The game uses the GoEmotions model; tests can pass in a fake.
public interface IEmotionScorer
{
    float[] Score(int[] tokenIds);
}

[Serializable]
public class AnswerEmotion
{
    public string valence = "";   // "positive", "neutral" or "negative"
    public string topEmotion = "";
    public float topScore;
    public float positiveScore;   // group scores: sum of the member labels' scores
    public float negativeScore;
    public float ambiguousScore;
    public float neutralScore;
}

[Serializable]
public class ReflectionEmotionResult
{
    public string overallValence = "";
    public List<AnswerEmotion> answers = new List<AnswerEmotion>();
}

//Classifies the player's reflection answers. Plain C# class (not a MonoBehaviour) so it can be tested without the game.
//Design: each answer is scored on its own; group scores are sums (Google's GoEmotions sentiment mapping);
//any negative answer makes the overall result negative (harm-first), otherwise any positive -> positive, else neutral.
public class MLAlgorithm
{
    public const string Positive = "positive";
    public const string Neutral = "neutral";
    public const string Negative = "negative";

    private static readonly HashSet<string> PositiveLabels = new HashSet<string>
    {
        "amusement", "excitement", "joy", "love", "desire", "optimism", "caring",
        "pride", "admiration", "gratitude", "relief", "approval"
    };
    private static readonly HashSet<string> NegativeLabels = new HashSet<string>
    {
        "fear", "nervousness", "remorse", "embarrassment", "disappointment",
        "sadness", "grief", "disgust", "anger", "annoyance", "disapproval"
    };
    private static readonly HashSet<string> AmbiguousLabels = new HashSet<string>
    {
        "realization", "surprise", "curiosity", "confusion"
    };

    private readonly RobertaTokenizer tokenizer;
    private readonly IEmotionScorer scorer;
    private readonly string[] labels;

    public MLAlgorithm(RobertaTokenizer tokenizer, IEmotionScorer scorer, IList<string> labels)
    {
        this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        this.scorer = scorer ?? throw new ArgumentNullException(nameof(scorer));
        this.labels = new List<string>(labels).ToArray();
    }

    public float[] ScoreText(string text)
    {
        float[] scores = scorer.Score(tokenizer.Encode(text));
        if (scores.Length != labels.Length)
        {
            throw new InvalidOperationException($"Model returned {scores.Length} scores for {labels.Length} labels.");
        }
        return scores;
    }

    public AnswerEmotion ScoreAnswer(string text)
    {
        return Summarize(ScoreText(text));
    }

    //Classifies each answer on its own, then combines them harm-first
    public ReflectionEmotionResult ClassifyAnswers(IList<string> answers)
    {
        ReflectionEmotionResult result = new ReflectionEmotionResult();
        foreach (string answer in answers)
        {
            result.answers.Add(ScoreAnswer(answer));
        }
        result.overallValence = CombineAnyNegative(result.answers);
        return result;
    }

    public AnswerEmotion Summarize(float[] scores)
    {
        AnswerEmotion emotion = new AnswerEmotion();
        int top = 0;
        for (int i = 0; i < scores.Length; i++)
        {
            string label = labels[i];
            if (PositiveLabels.Contains(label)) emotion.positiveScore += scores[i];
            else if (NegativeLabels.Contains(label)) emotion.negativeScore += scores[i];
            else if (AmbiguousLabels.Contains(label)) emotion.ambiguousScore += scores[i];
            else emotion.neutralScore += scores[i];

            if (scores[i] > scores[top]) top = i;
        }
        emotion.topEmotion = labels[top];
        emotion.topScore = scores[top];

        //Winning group; ambiguous counts as neutral. Ties go to the earlier group in this order.
        float best = emotion.positiveScore;
        emotion.valence = Positive;
        if (emotion.negativeScore > best) { best = emotion.negativeScore; emotion.valence = Negative; }
        if (emotion.ambiguousScore > best) { best = emotion.ambiguousScore; emotion.valence = Neutral; }
        if (emotion.neutralScore > best) { emotion.valence = Neutral; }
        return emotion;
    }

    public static string CombineAnyNegative(IList<AnswerEmotion> answers)
    {
        bool anyPositive = false;
        foreach (AnswerEmotion answer in answers)
        {
            if (answer.valence == Negative) return Negative;
            if (answer.valence == Positive) anyPositive = true;
        }
        return anyPositive ? Positive : Neutral;
    }
}

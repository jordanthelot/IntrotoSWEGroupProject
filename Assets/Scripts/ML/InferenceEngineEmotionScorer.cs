using System;
using Unity.InferenceEngine;
using UnityEngine;

//Runs the GoEmotions ONNX model with Unity Inference Engine and returns one score (0..1) per label.
//The model is multi-label, so each label's score comes from its own sigmoid.
public class InferenceEngineEmotionScorer : IEmotionScorer, IDisposable
{
    private readonly Worker worker;

    public InferenceEngineEmotionScorer(ModelAsset modelAsset, BackendType backend = BackendType.CPU)
    {
        if (modelAsset == null)
        {
            throw new ArgumentNullException(nameof(modelAsset));
        }
        worker = new Worker(ModelLoader.Load(modelAsset), backend);
    }

    public float[] Score(int[] tokenIds)
    {
        int length = tokenIds.Length;
        int[] mask = new int[length];
        for (int i = 0; i < length; i++)
        {
            mask[i] = 1;
        }

        using Tensor<int> inputIds = new Tensor<int>(new TensorShape(1, length), tokenIds);
        using Tensor<int> attentionMask = new Tensor<int>(new TensorShape(1, length), mask);
        worker.Schedule(inputIds, attentionMask);

        using Tensor<float> logits = (worker.PeekOutput("logits") as Tensor<float>).ReadbackAndClone();
        float[] scores = logits.DownloadToArray();
        for (int i = 0; i < scores.Length; i++)
        {
            scores[i] = 1f / (1f + Mathf.Exp(-scores[i]));
        }
        return scores;
    }

    public void Dispose()
    {
        worker?.Dispose();
    }
}

//Builds MLAlgorithm from the files in Assets/Resources/ReflectionML (exported by ML/export_unity_assets.py)
public static class ReflectionMLLoader
{
    public const string ResourceFolder = "ReflectionML/";

    //The scorer owns the model worker: dispose it when you're done with the algorithm
    public static MLAlgorithm Load(out InferenceEngineEmotionScorer scorer)
    {
        TextAsset vocab = Resources.Load<TextAsset>(ResourceFolder + "goemotions_vocab");
        TextAsset merges = Resources.Load<TextAsset>(ResourceFolder + "goemotions_merges");
        TextAsset labels = Resources.Load<TextAsset>(ResourceFolder + "goemotions_labels");
        ModelAsset model = Resources.Load<ModelAsset>(ResourceFolder + "goemotions");
        if (vocab == null || merges == null || labels == null || model == null)
        {
            throw new InvalidOperationException("ReflectionML files are missing from Assets/Resources/ReflectionML. " +
                                                "If goemotions.onnx is a small text file, run 'git lfs pull'.");
        }

        RobertaTokenizer tokenizer = RobertaTokenizer.FromText(vocab.text, merges.text);
        scorer = new InferenceEngineEmotionScorer(model);
        return new MLAlgorithm(tokenizer, scorer,
                               labels.text.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
    }
}

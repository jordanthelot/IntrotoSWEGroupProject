using System;
using System.Collections.Generic;
using UnityEngine;

// PBI 5 (Jordan's task "Trigger reflection activity at checkpoint", written ahead):
// when the player reaches a checkpoint, open the reflection question for that checkpoint.
//
// The questions listed here are PLACEHOLDERS. The real question/prompt content is PBI 8
// (Raul: "Create collection of reflection prompts and motivating quotes").
public class CheckpointReflectionTrigger : MonoBehaviour
{
    [Serializable]
    public class CheckpointQuestion
    {
        public string checkpointId;
        public string questionId;
        [TextArea] public string question;
    }

    [SerializeField] private List<CheckpointQuestion> questions = new List<CheckpointQuestion>();

    private void OnEnable() => Checkpoint.AnyReached += OnCheckpointReached;
    private void OnDisable() => Checkpoint.AnyReached -= OnCheckpointReached;

    private void OnCheckpointReached(Checkpoint checkpoint)
    {
        CheckpointQuestion entry = questions.Find(q => q.checkpointId == checkpoint.Id);
        if (entry == null) return; // no question at this checkpoint

        if (ReflectionQuestionUI.Instance == null)
        {
            Debug.LogError("CheckpointReflectionTrigger: no ReflectionQuestionUI in the scene.");
            return;
        }
        ReflectionQuestionUI.Instance.Show(entry.questionId, entry.question, checkpoint.Id);
    }
}

using System;
using System.Threading.Tasks;
using UnityEngine;

//a cycle represents three questions that culminate in a final reflection response from the user

public class ReflectionManager : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private CheckpointCycle saveService;//calls back to the file for the necessary saving/loading functions
    [SerializeField] private ReflectionUIManager reflectionUI;

    [Header("Testing")]
    private string cycleId;
    [SerializeField] private bool enableTestKeys = true;

    private CheckpointCycleVars currentCycle;

    private ReflectionPlayerProgress playerProgress = new ReflectionPlayerProgress();

    private bool isBusy;//true while a save/load is happening
    private bool isActivityOpen;//true while a question is progress or reflection panel is showing

    // Temporary questions until the question bank is ready.
    private readonly string[] testQuestions =
    {
        "How are you feeling today?",
        "What has been on your mind recently?",
        "What is one thing you would like to give yourself more time for?"
    };

    private void Awake()
    {
        if (saveService == null || reflectionUI == null)
        {
            Debug.LogError(
                "Assign the save service and reflection UI in the Inspector.");

            enabled = false;
            return;
        }

        // Listen for text submitted through the UI.
        reflectionUI.ResponseSubmitted += SubmitResponse;
    }

    private void Start()
    {
        LoadCycleAsync();
    }

    private void OnDestroy()
    {
        if (reflectionUI != null)
            reflectionUI.ResponseSubmitted -= SubmitResponse;
    }

    // Loads saved cycle progress and restores an unfinished screen.
    public async void LoadCycleAsync()
    {
        if (isBusy || isActivityOpen)
            return;

        isBusy = true;
        reflectionUI.SetBusy(true);

        string error = null;

        try
        {
            playerProgress = await saveService.LoadPlayerProgressAsync();
            cycleId = $"cycle_{playerProgress.activeCycleNumber:D2}";
            string json = await saveService.LoadReflectionAsync(cycleId);//making sure we load active cycle of question

            CheckpointCycleVars loaded;

            if (json == null)
            {
                loaded = new CheckpointCycleVars
                {
                    cycleId = cycleId
                };
            }
            else
            {   //checks to make sure the cycle is intact
                loaded =
                    JsonUtility.FromJson<CheckpointCycleVars>(json);

                if (loaded == null ||
                    loaded.cycleId != cycleId ||
                    loaded.questions == null ||
                    loaded.questions.Count > 3 ||
                    loaded.currentQuestionIndex < 0 ||
                    loaded.currentQuestionIndex > loaded.questions.Count)
                {
                    throw new InvalidOperationException(
                        "The saved cycle has an unsupported format.");
                }
            }

            // If the player was awaiting feedback from the AI and the game closes, the placeholder feedback is generated
            if (loaded.stage == ReflectionStage.AwaitingFeedback)
            {
                PrepareTestFeedback(loaded);
                await SaveCycleAsync(loaded);
            }
            else
            {
                currentCycle = loaded;
            }
            //flags to control at what stage of the cycle the game is in, i.e. is the player awaiting feedback, are they answering questions, etc.
            if (currentCycle.stage ==
                ReflectionStage.AwaitingPlayerReflection)
            {
                isActivityOpen = true;
                reflectionUI.ShowFeedback(currentCycle);
            }
            else if (
                currentCycle.stage == ReflectionStage.CollectingAnswers)
            {
                int index = currentCycle.currentQuestionIndex;

                if (index < currentCycle.questions.Count)
                {
                    CheckpointQuestionData question =
                        currentCycle.questions[index];

                    if (question.status == QuestionStatus.InProgress)
                    {
                        isActivityOpen = true;

                        reflectionUI.ShowReflection(
                            question.questionText,
                            question.answerText);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            error = "Could not load progress. Sign in, then press R to retry.";
        }
        finally
        {
            isBusy = false;
            reflectionUI.SetBusy(false);

            if (error != null)
                reflectionUI.ShowError(error);
        }
    }

    // Called by Checkpoint.cs or the T-key test.
    public async void OnCheckpointReached(string checkpointId)
    {
        if (isBusy || isActivityOpen)
            return;

        if (currentCycle == null)
        {
            reflectionUI.ShowError(
                "Progress is not loaded yet. Press R to retry loading.");
            return;
        }   

        //Don't repeat checkpoints from previous cycles
        if(playerProgress.answeredCheckpointIds.Contains((checkpointId)))
            return;

        //if we are not collecting answers and the current cycle is not yet completed, stop
        if (currentCycle.stage != ReflectionStage.CollectingAnswers && currentCycle.stage != ReflectionStage.Completed)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(checkpointId))
        {
            reflectionUI.ShowError("This checkpoint needs an ID.");
            return;
        }

        // If the checkpoint was already hit, but questions remained unanswerd, reopen it. Otherwise, do nothing.
        CheckpointQuestionData existing =
            currentCycle.questions.Find(
                question => question.checkpointId == checkpointId);

        if (existing != null)
        {
            if (existing.status == QuestionStatus.InProgress)
            {
                isActivityOpen = true;

                reflectionUI.ShowReflection(
                    existing.questionText,
                    existing.answerText);
            }

            return;
        }

        if (currentCycle.stage == ReflectionStage.CollectingAnswers && currentCycle.questions.Count >= 3)//last check is to make sure a fourth question isn't added to the cycle
            return;

        isBusy = true;
        reflectionUI.SetBusy(true);

        string error = null;

        try
        {
            bool startingNewCycle = currentCycle.stage == ReflectionStage.Completed;
            int nextCycleNumber = playerProgress.activeCycleNumber;
            //Copies the cycle data
            CheckpointCycleVars updated;

            if (startingNewCycle)
            {
                nextCycleNumber++;
                updated = new CheckpointCycleVars
                {
                    cycleId = $"cycle_{nextCycleNumber:D2}",
                    stage = ReflectionStage.CollectingAnswers
                };
            }
            else
            {
                updated = JsonUtility.FromJson<CheckpointCycleVars>(JsonUtility.ToJson(currentCycle));
            }
                

            int index = updated.questions.Count;
            //Creates a new InProgress question, and only shows said question on screen
            CheckpointQuestionData question = new CheckpointQuestionData
            {
                checkpointId = checkpointId,
                questionId = $"test_question_{index + 1}",
                questionText = testQuestions[index],
                status = QuestionStatus.InProgress
            };

            updated.questions.Add(question);
            updated.currentQuestionIndex = index;



            await SaveCycleAsync(updated, nextCycleNumber);

            isActivityOpen = true;
            reflectionUI.ShowReflection(question.questionText);//shows question on reflectionUI
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            error = "Could not start the question. Re-enter the checkpoint to retry.";
        }
        finally
        {
            isBusy = false;
            reflectionUI.SetBusy(false);

            if (error != null)
                reflectionUI.ShowError(error);
        }
    }

    // Handles both checkpoint answers and the final reflection. 
    private async void SubmitResponse(string answerText)
    {
        if (isBusy || !isActivityOpen || currentCycle == null)
            return;

        if (string.IsNullOrWhiteSpace(answerText))
            return;

        isBusy = true;
        reflectionUI.SetBusy(true);

        string error = null;

        try
        {
            // Keep the original cycle intact if the save fails.
            CheckpointCycleVars updated =
                JsonUtility.FromJson<CheckpointCycleVars>(
                    JsonUtility.ToJson(currentCycle));

            if (updated.stage == ReflectionStage.CollectingAnswers)
            {
                int index = updated.currentQuestionIndex;

                if (index < 0 || index >= updated.questions.Count)
                    throw new InvalidOperationException("Invalid question index.");

                CheckpointQuestionData question = updated.questions[index];

                if (question.status != QuestionStatus.InProgress)
                    throw new InvalidOperationException("Question is not active.");
                //Update question status to answered, update the question index
                question.answerText = answerText.Trim();
                question.status = QuestionStatus.Answered;

                updated.currentQuestionIndex = index + 1;

                bool allThreeAnswered =
                    updated.questions.Count == 3 &&
                    updated.questions.TrueForAll(
                        item => item.status == QuestionStatus.Answered);

                //This feedback is supposed to be the AI's response
                if (allThreeAnswered)
                    PrepareTestFeedback(updated);
            }
            //if the submit button is hit after the player submits there reflection, process is complete
            else if (
                updated.stage == ReflectionStage.AwaitingPlayerReflection)
            {
                updated.playerReflectionText = answerText.Trim();
                updated.stage = ReflectionStage.Completed;
            }
            else
            {
                return;
            }

            await SaveCycleAsync(updated);

            if (currentCycle.stage ==
                ReflectionStage.AwaitingPlayerReflection)
            {
                // After answer three, keep the panel open for reflection.
                reflectionUI.ShowFeedback(currentCycle);
            }
            else
            {
                // After answers one/two, or the final reflection.
                isActivityOpen = false;
                reflectionUI.CloseReflection();

                if (currentCycle.stage == ReflectionStage.Completed)
                    Debug.Log("Reflection cycle completed and saved.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            error = "Could not save. Your text is still here; press Submit to retry.";
        }
        finally
        {
            isBusy = false;
            reflectionUI.SetBusy(false);

            if (error != null)
                reflectionUI.ShowError(error);
        }
    }

    // Temporary content, not an actual AI request.
    private void PrepareTestFeedback(CheckpointCycleVars cycle)
    {
        cycle.AIResponse =
            "[TEST FEEDBACK — NOT GENERATED BY AI]\n" +
            "Thank you for answering the three questions. " +
            "Which answer stands out to you, and why?";

        cycle.quoteText =
            "You can take one small step without knowing the whole path.";

        cycle.quoteAttribution = "Original placeholder text";

        cycle.stage = ReflectionStage.AwaitingPlayerReflection;
    }

    private async Task SaveCycleAsync(CheckpointCycleVars updated, int? newCyclenumber = null)//what actually converts the cycle into a JSON object
    {
        //Work on a copy
        ReflectionPlayerProgress updatedProgress = JsonUtility.FromJson<ReflectionPlayerProgress>(JsonUtility.ToJson(playerProgress));

        if (newCyclenumber.HasValue)
        {
            updatedProgress.activeCycleNumber = newCyclenumber.Value;
        }
            //Only including questions which have been answered
            foreach (CheckpointQuestionData question in updated.questions)
            {
                if (question.status == QuestionStatus.Answered && !updatedProgress.answeredCheckpointIds.Contains(question.checkpointId))
                {
                    updatedProgress.answeredCheckpointIds.Add(question.checkpointId);
                }
            }
            await saveService.SaveCycleAndPlayerProgressAsync(updated, updatedProgress);

            currentCycle = updated;
            playerProgress = updatedProgress;
            cycleId = updated.cycleId;
        
    }

    private void Update()
    {
        if (!enableTestKeys || isBusy || isActivityOpen)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            LoadCycleAsync();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            if (currentCycle == null)
            {
                reflectionUI.ShowError("Press R to load the cycle first.");
                return;
            }

            int nextCheckpointNumber;// initialize for testing

            if (currentCycle.stage == ReflectionStage.Completed)
            {
                //Cycle 1 -> completed at checkpiont 4
                nextCheckpointNumber = playerProgress.activeCycleNumber * 3 + 1;
            }
            else
            {
                nextCheckpointNumber = (playerProgress.activeCycleNumber - 1) * 3 + currentCycle.questions.Count + 1;
            }



            OnCheckpointReached(
                $"test_checkpoint_{nextCheckpointNumber}");
        }
    }
}



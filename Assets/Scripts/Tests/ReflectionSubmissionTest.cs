using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// PBI 6: Validate player responses (Soni), tested on its own and inside the reflection panel.
// Part 1 tests ResponseValidator directly.
// Part 2 loads the real Gameplay scene and uses the reflection panel (ReflectionUIManager):
// bad answers are blocked with a message, a good answer is trimmed and handed to
// ReflectionManager through ResponseSubmitted.
// Jordan's ReflectionManager is removed for Part 2 so this test never writes to Cloud Save.
//
// How to run: open Assets/Scenes/Tests/GameplayTests and press Play. Read the Console.
public class ReflectionSubmissionTest : MonoBehaviour
{
    private readonly TestReport report = new TestReport("ReflectionSubmissionTest");

    private IEnumerator Start()
    {
        Application.runInBackground = true; // keep running when Unity isn't the front window
        DontDestroyOnLoad(gameObject);
        Time.timeScale = 1f;

        // ---------- Part 1: validator ----------
        Reject("Empty answer is rejected", "");
        Reject("Spaces-only answer is rejected", "     ");
        Reject("Punctuation-only answer is rejected", "!!! ...");
        Reject("Answer over 500 characters is rejected", new string('a', ResponseValidator.MaxLength + 1));
        bool ok = ResponseValidator.Validate("  I was bored after class  ", out string cleaned, out string err);
        report.Record("Normal answer is accepted", ok, err ?? "");
        report.Record("Answer is trimmed", cleaned == "I was bored after class", $"'{cleaned}'");
        report.Record("Exactly 500 characters is accepted",
            ResponseValidator.Validate(new string('a', ResponseValidator.MaxLength), out _, out _));
        report.Record("Numbers-only answer is accepted", ResponseValidator.Validate("3", out _, out _));

        // ---------- Part 2: the reflection panel in Gameplay ----------
        if (!Application.CanStreamedLevelBeLoaded("Gameplay"))
        {
            report.Record("Gameplay scene is in the build scene list", false);
            report.Summary();
            yield break;
        }
        SceneManager.LoadScene("Gameplay");
        yield return null;

        ReflectionManager manager = FindAnyObjectByType<ReflectionManager>();
        if (manager != null) Destroy(manager); // unsubscribes it from the panel; no Cloud Save calls during the test
        yield return null;
        yield return new WaitForSecondsRealtime(0.5f);

        ReflectionUIManager ui = FindAnyObjectByType<ReflectionUIManager>();
        GameObject player = GameObject.FindWithTag("Player");
        report.Record("Gameplay: reflection panel and player found", ui != null && player != null);
        if (ui == null || player == null) { report.Summary(); yield break; }
        PlayerController controls = player.GetComponent<PlayerController>();

        int sent = 0;
        string lastSent = null;
        ui.ResponseSubmitted += r => { sent++; lastSent = r; };

        ui.ShowReflection("How do you usually feel after scrolling?");
        yield return null;
        report.Record("Gameplay: question opens, game pauses, controls off",
            ui.IsOpen && ui.reflectionPanel.activeSelf && Time.timeScale == 0f && controls != null && !controls.enabled);

        Submit(ui, "");
        report.Record("Gameplay: empty answer is blocked", sent == 0 && ui.IsOpen);
        report.Record("Gameplay: player is told why", ui.statusText == null || !string.IsNullOrEmpty(ui.statusText.text),
            ui.statusText != null ? $"'{ui.statusText.text}'" : "no status text in scene");

        Submit(ui, "?!?!");
        report.Record("Gameplay: punctuation-only answer is blocked", sent == 0 && ui.IsOpen);

        Submit(ui, new string('b', ResponseValidator.MaxLength + 1));
        report.Record("Gameplay: over-500-character answer is blocked", sent == 0 && ui.IsOpen);

        Submit(ui, "  I scroll when I'm bored  ");
        report.Record("Gameplay: valid answer is sent to ReflectionManager, trimmed",
            sent == 1 && lastSent == "I scroll when I'm bored", $"sent {sent}, '{lastSent}'");

        ui.CloseReflection();
        yield return null;
        report.Record("Gameplay: closing the panel resumes the game and controls",
            !ui.IsOpen && Time.timeScale == 1f && controls.enabled);

        report.Summary();
    }

    private static void Submit(ReflectionUIManager ui, string text)
    {
        ui.reflectionInputField.text = text;
        ui.SubmitReflection();
    }

    private void Reject(string caseName, string answer)
    {
        bool accepted = ResponseValidator.Validate(answer, out _, out string error);
        report.Record(caseName, !accepted && !string.IsNullOrEmpty(error), accepted ? "was accepted" : "");
    }
}

using System;
using UnityEngine;

// Tests PBI 6: response validation and response submission.
public class ReflectionSubmissionTest : MonoBehaviour
{
    private readonly TestReport report = new TestReport("ReflectionSubmissionTest");

    private void Start()
    {
        ReflectionResponse fromEvent = null;
        Action<ReflectionResponse> onSubmitted = r => fromEvent = r;
        ReflectionSubmitter.Submitted += onSubmitted;
        int startCount = ReflectionSubmitter.SessionResponses.Count;

        Reject("Empty answer is rejected", "");
        Reject("Spaces-only answer is rejected", "     ");
        Reject("Punctuation-only answer is rejected", "!!! ...");
        Reject("Answer over 500 characters is rejected", new string('a', ResponseValidator.MaxLength + 1));
        report.Record("Missing question is rejected",
            !ReflectionSubmitter.TrySubmit("", "Q", "CP-1", "A real answer", out _, out _));
        report.Record("Rejected answers are not saved", ReflectionSubmitter.SessionResponses.Count == startCount);

        DateTime before = DateTime.UtcNow;
        bool ok = ReflectionSubmitter.TrySubmit("q-why-scroll", "Why did you open Instagram?", "CP-1",
                                                "  I was bored after class  ", out ReflectionResponse r, out string err);
        report.Record("Valid answer is accepted", ok, err ?? "");
        report.Record("Answer is trimmed", ok && r.answer == "I was bored after class", ok ? $"'{r.answer}'" : "");
        report.Record("Question and checkpoint are attached", ok && r.questionId == "q-why-scroll" && r.checkpointId == "CP-1");
        bool timeOk = ok && DateTime.TryParse(r.timestampUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime ts)
                      && (ts - before).TotalSeconds > -1 && (ts - before).TotalSeconds < 5;
        report.Record("Date/time is recorded", timeOk, ok ? r.timestampUtc : "");
        report.Record("Accepted answer is kept for this session", ReflectionSubmitter.SessionResponses.Count == startCount + 1);
        report.Record("Submitted event fires (for the database in PBI 7)", fromEvent != null && fromEvent == r);
        report.Record("Exactly 500 characters is accepted",
            ReflectionSubmitter.TrySubmit("q-len", "Q", "CP-1", new string('a', ResponseValidator.MaxLength), out _, out _));

        ReflectionSubmitter.Submitted -= onSubmitted;
        report.Summary();
    }

    private void Reject(string caseName, string answer)
    {
        bool accepted = ReflectionSubmitter.TrySubmit("q-test", "Test question", "CP-1", answer, out _, out string error);
        report.Record(caseName, !accepted && !string.IsNullOrEmpty(error), accepted ? "was accepted" : "");
    }
}

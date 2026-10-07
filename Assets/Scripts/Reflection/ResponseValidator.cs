// PBI 6: Validate player responses.
// Rules for a reflection answer:
//   - Leading/trailing spaces are removed.
//   - It can't be empty.
//   - It must contain at least one letter or number (rejects "!!!" or "...").
//   - It can be at most MaxLength characters.
public static class ResponseValidator
{
    public const int MaxLength = 500;

    public static bool Validate(string rawAnswer, out string cleanedAnswer, out string error)
    {
        cleanedAnswer = rawAnswer == null ? "" : rawAnswer.Trim();

        if (cleanedAnswer.Length == 0)
        {
            error = "Please write an answer before submitting.";
            return false;
        }

        if (cleanedAnswer.Length > MaxLength)
        {
            error = $"Please keep your answer under {MaxLength} characters.";
            return false;
        }

        bool hasLetterOrDigit = false;
        foreach (char c in cleanedAnswer)
        {
            if (char.IsLetterOrDigit(c)) { hasLetterOrDigit = true; break; }
        }
        if (!hasLetterOrDigit)
        {
            error = "Please answer using words.";
            return false;
        }

        error = null;
        return true;
    }
}

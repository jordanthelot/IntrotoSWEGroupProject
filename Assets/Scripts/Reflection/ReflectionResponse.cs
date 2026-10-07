using System;

// One answer to one reflection question. This is what gets stored in the database (PBI 7).
[Serializable]
public class ReflectionResponse
{
    public string questionId;
    public string questionText;
    public string answer;
    public string checkpointId;
    public string timestampUtc; // ISO 8601, e.g. 2026-10-07T15:31:00.0000000Z
}

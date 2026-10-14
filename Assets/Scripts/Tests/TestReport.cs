using System.Collections.Generic;
using UnityEngine;

// Shared PASS/FAIL logging for the in-game test runners.
public class TestReport
{
    private readonly string name;
    private readonly List<string> lines = new List<string>();
    public int Passed { get; private set; }
    public int Failed { get; private set; }

    public TestReport(string name) { this.name = name; }

    public void Record(string caseName, bool ok, string detail = "")
    {
        if (ok) Passed++; else Failed++;
        string line = $"{(ok ? "PASS" : "FAIL")}  {caseName}{(ok || detail == "" ? "" : "  (" + detail + ")")}";
        lines.Add(line);
        if (ok) Debug.Log($"{name}: {line}"); else Debug.LogError($"{name}: {line}");
    }

    public void Summary()
    {
        string text = $"{name}: {Passed} passed, {Failed} failed\n" + string.Join("\n", lines);
        if (Failed == 0) Debug.Log(text); else Debug.LogError(text);
    }
}

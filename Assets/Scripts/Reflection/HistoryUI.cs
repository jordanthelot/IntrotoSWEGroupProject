using UnityEngine;
using System;
using System.Globalization;
using System.Text;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;


public class HistoryUI : MonoBehaviour
{
    [SerializeField] private GameObject historypan;
    [SerializeField] private TMP_Text historytext;
    [SerializeField] private ScrollRect historyScroll;
    [SerializeField] private CheckpointCycle saveserv;

    private int loadversion;

    private void Awake()
    {
        historytext.richText = false;
        historypan.SetActive(false);
    }

    public async void OpenHistory()
    {
        int version = ++loadversion;

        historypan.SetActive(true);
        historytext.text = "Loading reflections...";

        if (UnityServices.State != ServicesInitializationState.Initialized || !AuthenticationService.Instance.IsSignedIn)
        {
            historytext.text = "Please sign in to view history.";
            return;
        }

        string playerid = AuthenticationService.Instance.PlayerId;
        try
        {
            var history = await saveserv.LoadReflectionHistoryAsync();
            if (this == null || version != loadversion)
            {
                return;
            }
            if (!AuthenticationService.Instance.IsSignedIn || AuthenticationService.Instance.PlayerId != playerid)
            {
                historytext.text = "Sign in to view your reflection";
                return;
            }
            if (history.Count == 0)
            {
                historytext.text = "No completed reflections yet";
                return;
            }

            var text = new StringBuilder();
            foreach (var cycle in history)
            {
                if (text.Length > 0)
                    text.Append("\n\n————————————\n\n");

                if (DateTimeOffset.TryParse(
                    cycle.completedAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var completed))
                {
                    text.AppendLine(
                        completed.ToLocalTime().ToString(
                            "MMM d, yyyy • h:mm tt",
                            CultureInfo.InvariantCulture));
                    text.AppendLine();
                }

                text.AppendLine("AI response");
                text.AppendLine(cycle.AIResponse.Trim());
                text.AppendLine();
                text.AppendLine("Your reflection");
                text.AppendLine(
                    string.IsNullOrWhiteSpace(cycle.playerReflectionText)
                        ? "(No reply saved.)"
                        : cycle.playerReflectionText.Trim());
            }

            historytext.text = text.ToString();

            Canvas.ForceUpdateCanvases();
            historyScroll.StopMovement();
            historyScroll.verticalNormalizedPosition = 1f;
        }
        catch (Exception)
        {
            if (this == null || version != loadversion)
                return;

            historytext.text =
                "Couldn't load your reflections. Close this panel and try again.";

            Debug.LogWarning("Reflection history could not be loaded.");
        }
    }
    public void CloseHistory()
    {
        ++loadversion;
        historytext.text = "";
        historypan.SetActive(false);
            }
        }

    



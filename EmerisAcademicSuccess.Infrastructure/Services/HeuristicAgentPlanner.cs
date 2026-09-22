using System.Globalization;
using System.Text.RegularExpressions;
using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class HeuristicAgentPlanner
{
    private static readonly Regex DateTimeRegex = new(@"(?<value>\d{1,2}[/-]\d{1,2}[/-]\d{2,4}(?:\s+\d{1,2}:\d{2})?|\d{1,2}\s+[A-Za-z]{3,9}\s+\d{4}(?:\s+\d{1,2}:\d{2})?)", RegexOptions.Compiled);

    public AgentPlanResult CreatePlan(string message, DateTime currentLocalTime)
    {
        var lowered = message.ToLowerInvariant();
        var actions = new List<AgentActionRequest>();
        var needsRag = lowered.Contains("study") || lowered.Contains("guide") || lowered.Contains("assessment") || lowered.Contains("timetable") || lowered.Contains("schedule");
        var needsStudyPlan = lowered.Contains("study plan") || lowered.Contains("revision plan") || lowered.Contains("study schedule");

        if (lowered.Contains("date") || lowered.Contains("today") || lowered.Contains("time") || lowered.Contains("deadline"))
        {
            actions.Add(new AgentActionRequest("get_current_date", new Dictionary<string, string>()));
        }

        if (lowered.Contains("remind") || lowered.Contains("reminder"))
        {
            var dueAt = TryExtractDate(message, currentLocalTime);
            var title = ExtractReminderTitle(message);

            if (dueAt is not null && !string.IsNullOrWhiteSpace(title))
            {
                actions.Add(new AgentActionRequest(
                    "create_reminder",
                    new Dictionary<string, string>
                    {
                        ["title"] = title,
                        ["dueAt"] = dueAt.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                    }));
            }
            else
            {
                actions.Add(new AgentActionRequest("list_reminders", new Dictionary<string, string>()));
            }
        }

        if (lowered.Contains("schedule") || lowered.Contains("assessment") || lowered.Contains("timetable"))
        {
            actions.Add(new AgentActionRequest("get_schedule_overview", new Dictionary<string, string>()));
        }

        if (needsRag)
        {
            actions.Add(new AgentActionRequest(
                "search_documents",
                new Dictionary<string, string> { ["query"] = message }));
        }

        if (actions.Count == 0)
        {
            actions.Add(new AgentActionRequest(
                "search_documents",
                new Dictionary<string, string> { ["query"] = message }));
            needsRag = true;
        }

        return new AgentPlanResult(
            "student_support",
            "Fallback heuristic planning was used because no Gemini plan was available.",
            actions,
            needsRag,
            needsStudyPlan);
    }

    private static DateTime? TryExtractDate(string message, DateTime currentLocalTime)
    {
        if (message.Contains("tomorrow", StringComparison.OrdinalIgnoreCase))
        {
            return currentLocalTime.Date.AddDays(1).AddHours(18);
        }

        if (message.Contains("today", StringComparison.OrdinalIgnoreCase))
        {
            return currentLocalTime.Date.AddHours(currentLocalTime.Hour < 18 ? 18 : currentLocalTime.Hour + 1);
        }

        var match = DateTimeRegex.Match(message);
        if (!match.Success)
        {
            return null;
        }

        var formats = new[]
        {
            "d/M/yyyy H:mm",
            "dd/MM/yyyy H:mm",
            "d-M-yyyy H:mm",
            "dd-MM-yyyy H:mm",
            "d MMM yyyy H:mm",
            "dd MMM yyyy H:mm",
            "d/M/yyyy",
            "dd/MM/yyyy",
            "d-M-yyyy",
            "dd-MM-yyyy",
            "d MMM yyyy",
            "dd MMM yyyy"
        };

        if (DateTime.TryParseExact(match.Groups["value"].Value, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
        {
            return parsed.TimeOfDay == TimeSpan.Zero ? parsed.Date.AddHours(18) : parsed;
        }

        return DateTime.TryParse(match.Groups["value"].Value, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out parsed)
            ? parsed
            : null;
    }

    private static string ExtractReminderTitle(string message)
    {
        var cleaned = Regex.Replace(message, @"(?i)remind me( to)?", string.Empty).Trim();
        cleaned = Regex.Replace(cleaned, @"(?i)(on|at|by)\s+.+$", string.Empty).Trim();
        return cleaned.Trim(' ', '-', ':', '.');
    }
}
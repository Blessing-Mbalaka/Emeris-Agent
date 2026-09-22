using System.Globalization;
using System.Text.RegularExpressions;
using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class ScheduleHeuristicParser
{
    private static readonly Regex TimeRangeRegex = new(@"(?<start>\d{1,2}[:.]\d{2})\s*(?:-|to|–)\s*(?<end>\d{1,2}[:.]\d{2})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DayRegex = new(@"\b(?<day>Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex AssessmentKeywordRegex = new(@"\b(test|quiz|exam|assignment|assessment|practical|project)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DateRegex = new(@"(?<date>\d{1,2}[/-]\d{1,2}[/-]\d{2,4}|\d{1,2}\s+[A-Za-z]{3,9}\s+\d{4})", RegexOptions.Compiled);
    private static readonly Regex ModuleRegex = new(@"\b(?<module>[A-Z]{2,6}\d{0,4})\b", RegexOptions.Compiled);

    public IReadOnlyList<ScheduleEntry> Parse(DocumentCategory category, string extractedText)
    {
        var lines = extractedText
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return category switch
        {
            DocumentCategory.Timetable => ParseTimetable(lines),
            DocumentCategory.AssessmentSchedule => ParseAssessments(lines),
            _ => Array.Empty<ScheduleEntry>()
        };
    }

    public string BuildSummary(DocumentCategory category, IReadOnlyList<ScheduleEntry> entries, string extractedText)
    {
        if (entries.Count == 0)
        {
            var preview = extractedText.Length <= 420 ? extractedText : extractedText[..420] + "...";
            return category switch
            {
                DocumentCategory.StudyGuide => $"Study guide indexed for retrieval. Preview: {preview}",
                _ => $"Document text extracted, but table rows were not confidently detected. Preview: {preview}"
            };
        }

        return string.Join(
            Environment.NewLine,
            entries.Take(6).Select(entry =>
            {
                var when = entry.OccursOn is not null
                    ? entry.OccursOn.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
                    : entry.DayOfWeek?.ToString() ?? "TBA";
                var time = entry.StartTime is not null ? $" {entry.StartTime:HH:mm}" : string.Empty;
                var end = entry.EndTime is not null ? $"-{entry.EndTime:HH:mm}" : string.Empty;
                return $"- {when}{time}{end}: {entry.Title}";
            }));
    }

    private static IReadOnlyList<ScheduleEntry> ParseTimetable(IEnumerable<string> lines)
    {
        var entries = new List<ScheduleEntry>();

        foreach (var line in lines)
        {
            if (!TimeRangeRegex.IsMatch(line))
            {
                continue;
            }

            var timeMatch = TimeRangeRegex.Match(line);
            var dayMatch = DayRegex.Match(line);
            var startTime = TryParseTime(timeMatch.Groups["start"].Value);
            var endTime = TryParseTime(timeMatch.Groups["end"].Value);
            var title = line;
            title = DayRegex.Replace(title, string.Empty);
            title = TimeRangeRegex.Replace(title, string.Empty);
            title = Regex.Replace(title, @"\s{2,}", " ").Trim(' ', '-', '|', ':');

            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            entries.Add(new ScheduleEntry
            {
                EntryType = ScheduleEntryType.TimetableSession,
                Title = title,
                ModuleCode = ExtractModule(title),
                DayOfWeek = TryParseDay(dayMatch.Success ? dayMatch.Groups["day"].Value : null),
                StartTime = startTime,
                EndTime = endTime,
                Venue = ExtractVenue(line, title)
            });
        }

        return entries;
    }

    private static IReadOnlyList<ScheduleEntry> ParseAssessments(IEnumerable<string> lines)
    {
        var entries = new List<ScheduleEntry>();

        foreach (var line in lines)
        {
            if (!AssessmentKeywordRegex.IsMatch(line))
            {
                continue;
            }

            var dateMatch = DateRegex.Match(line);
            var date = dateMatch.Success ? TryParseDate(dateMatch.Groups["date"].Value) : null;
            var cleaned = DateRegex.Replace(line, string.Empty);
            cleaned = Regex.Replace(cleaned, @"\s{2,}", " ").Trim(' ', '-', '|', ':');

            entries.Add(new ScheduleEntry
            {
                EntryType = ScheduleEntryType.Assessment,
                Title = cleaned,
                ModuleCode = ExtractModule(cleaned),
                OccursOn = date,
                Notes = line
            });
        }

        return entries;
    }

    private static DayOfWeek? TryParseDay(string? input)
    {
        return Enum.TryParse<DayOfWeek>(input, true, out var value) ? value : null;
    }

    private static TimeOnly? TryParseTime(string input)
    {
        var normalized = input.Replace('.', ':');
        return TimeOnly.TryParseExact(normalized, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            || TimeOnly.TryParseExact(normalized, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out value)
            ? value
            : null;
    }

    private static DateOnly? TryParseDate(string input)
    {
        var formats = new[] { "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "dd-MM-yyyy", "d MMM yyyy", "dd MMM yyyy", "d MMMM yyyy", "dd MMMM yyyy" };
        return DateOnly.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;
    }

    private static string? ExtractModule(string text)
    {
        var match = ModuleRegex.Match(text);
        return match.Success ? match.Groups["module"].Value : null;
    }

    private static string? ExtractVenue(string line, string title)
    {
        var stripped = line.Replace(title, string.Empty, StringComparison.OrdinalIgnoreCase);
        var fragments = stripped.Split(new[] { "  ", "\t" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return fragments.LastOrDefault(fragment => !TimeRangeRegex.IsMatch(fragment) && !DayRegex.IsMatch(fragment));
    }
}
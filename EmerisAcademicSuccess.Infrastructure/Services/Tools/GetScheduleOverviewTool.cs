using System.Globalization;
using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Infrastructure.Services.Tools;

public sealed class GetScheduleOverviewTool(IDocumentRepository documentRepository) : IAgentTool
{
    public string Name => "get_schedule_overview";

    public string Description => "Returns upcoming timetable and assessment schedule items extracted from uploaded PDFs.";

    public async Task<AgentActionResult> ExecuteAsync(IReadOnlyDictionary<string, string> arguments, AgentExecutionContext context, CancellationToken cancellationToken)
    {
        var entries = await documentRepository.GetScheduleEntriesAsync(cancellationToken);
        if (entries.Count == 0)
        {
            return new AgentActionResult(Name, true, "No schedule entries have been extracted yet.");
        }

        var summary = string.Join(
            "; ",
            entries.Take(5).Select(entry => Describe(entry)));
        return new AgentActionResult(Name, true, $"Upcoming schedule snapshot: {summary}.");
    }

    private static string Describe(ScheduleEntry entry)
    {
        var datePart = entry.OccursOn is not null
            ? entry.OccursOn.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
            : entry.DayOfWeek?.ToString() ?? "TBA";
        var timePart = entry.StartTime is not null ? $" {entry.StartTime:HH:mm}" : string.Empty;
        return $"{datePart}{timePart} {entry.Title}".Trim();
    }
}
using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Infrastructure.Services.Tools;

public sealed class ListRemindersTool(IReminderService reminderService) : IAgentTool
{
    public string Name => "list_reminders";

    public string Description => "Lists upcoming reminders already stored in the system.";

    public async Task<AgentActionResult> ExecuteAsync(IReadOnlyDictionary<string, string> arguments, AgentExecutionContext context, CancellationToken cancellationToken)
    {
        var reminders = await reminderService.GetUpcomingAsync(5, cancellationToken);
        if (reminders.Count == 0)
        {
            return new AgentActionResult(Name, true, "There are no upcoming reminders yet.");
        }

        var summary = string.Join(", ",
            reminders.Select(reminder => $"{reminder.Title} at {reminder.DueAtLocal:g}"));
        return new AgentActionResult(Name, true, $"Upcoming reminders: {summary}.");
    }
}
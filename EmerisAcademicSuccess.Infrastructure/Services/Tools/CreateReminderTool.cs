using System.Globalization;
using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Infrastructure.Services.Tools;

public sealed class CreateReminderTool(IReminderService reminderService) : IAgentTool
{
    public string Name => "create_reminder";

    public string Description => "Creates a reminder when the user supplies a clear title and due date/time.";

    public async Task<AgentActionResult> ExecuteAsync(IReadOnlyDictionary<string, string> arguments, AgentExecutionContext context, CancellationToken cancellationToken)
    {
        if (!arguments.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title))
        {
            return new AgentActionResult(Name, false, "Reminder title is missing.");
        }

        if (!arguments.TryGetValue("dueAt", out var dueAtRaw)
            || !DateTime.TryParse(dueAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dueAt))
        {
            return new AgentActionResult(Name, false, "Reminder date/time is missing or invalid.");
        }

        arguments.TryGetValue("notes", out var notes);

        var reminder = await reminderService.CreateAsync(new CreateReminderRequest(title, dueAt, notes), cancellationToken);
        return new AgentActionResult(Name, true, $"Reminder created for '{reminder.Title}' at {reminder.DueAtLocal:g}.");
    }
}
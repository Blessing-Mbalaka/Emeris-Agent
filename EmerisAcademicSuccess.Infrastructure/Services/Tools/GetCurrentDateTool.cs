using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Infrastructure.Services.Tools;

public sealed class GetCurrentDateTool(ISystemClock clock) : IAgentTool
{
    public string Name => "get_current_date";

    public string Description => "Returns the current local date and time for planning reminders and study plans.";

    public Task<AgentActionResult> ExecuteAsync(IReadOnlyDictionary<string, string> arguments, AgentExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new AgentActionResult(Name, true, $"Current local date and time is {clock.LocalNow:dddd, dd MMM yyyy HH:mm}."));
    }
}
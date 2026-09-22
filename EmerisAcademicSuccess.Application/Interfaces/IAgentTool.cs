using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IAgentTool
{
    string Name { get; }
    string Description { get; }
    Task<AgentActionResult> ExecuteAsync(
        IReadOnlyDictionary<string, string> arguments,
        AgentExecutionContext context,
        CancellationToken cancellationToken);
}
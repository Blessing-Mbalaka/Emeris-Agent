using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IAgentOrchestrator
{
    Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken);
}
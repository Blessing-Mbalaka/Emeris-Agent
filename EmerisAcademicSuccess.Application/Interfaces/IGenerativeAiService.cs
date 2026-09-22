using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IGenerativeAiService
{
    Task<AgentPlanResult?> PlanAsync(AgentPromptContext context, string apiKey, CancellationToken cancellationToken);
    Task<string?> ComposeResponseAsync(AgentResponseContext context, string apiKey, CancellationToken cancellationToken);
    Task<IReadOnlyList<float>?> CreateEmbeddingAsync(string text, string apiKey, CancellationToken cancellationToken);
}
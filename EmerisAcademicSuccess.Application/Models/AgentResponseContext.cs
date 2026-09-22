namespace EmerisAcademicSuccess.Application.Models;

public sealed record AgentResponseContext(
    string UserMessage,
    DateTime CurrentLocalTime,
    AgentPlanResult Plan,
    IReadOnlyList<AgentActionResult> Actions,
    RagSearchResult RagResult);
namespace EmerisAcademicSuccess.Application.Models;

public sealed record AgentResponse(
    string FinalResponse,
    string ReasoningSummary,
    IReadOnlyList<AgentActionResult> Actions,
    IReadOnlyList<RagCitation> Citations,
    string SearchMode);
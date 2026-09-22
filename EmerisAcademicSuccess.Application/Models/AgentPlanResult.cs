namespace EmerisAcademicSuccess.Application.Models;

public sealed record AgentPlanResult(
    string Intent,
    string Reasoning,
    IReadOnlyList<AgentActionRequest> Actions,
    bool NeedsRag,
    bool NeedsStudyPlan);
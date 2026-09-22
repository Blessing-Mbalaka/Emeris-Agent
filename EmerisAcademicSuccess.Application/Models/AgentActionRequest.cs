namespace EmerisAcademicSuccess.Application.Models;

public sealed record AgentActionRequest(
    string ToolName,
    IReadOnlyDictionary<string, string> Arguments);
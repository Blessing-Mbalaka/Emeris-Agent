namespace EmerisAcademicSuccess.Application.Models;

public sealed record AgentActionResult(
    string ToolName,
    bool Success,
    string Message);
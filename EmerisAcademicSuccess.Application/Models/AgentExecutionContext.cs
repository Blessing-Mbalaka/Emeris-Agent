namespace EmerisAcademicSuccess.Application.Models;

public sealed record AgentExecutionContext(
    string UserMessage,
    string? ApiKey,
    DateTime CurrentLocalTime);
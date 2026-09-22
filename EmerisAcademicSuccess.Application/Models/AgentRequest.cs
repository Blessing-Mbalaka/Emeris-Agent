namespace EmerisAcademicSuccess.Application.Models;

public sealed record AgentRequest(
    string Message,
    string? ApiKeyOverride,
    Guid ConversationId);
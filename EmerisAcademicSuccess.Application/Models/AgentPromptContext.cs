namespace EmerisAcademicSuccess.Application.Models;

public sealed record AgentPromptContext(
    string UserMessage,
    DateTime CurrentLocalTime,
    IReadOnlyList<string> AvailableTools,
    IReadOnlyList<string> UploadedDocuments,
    IReadOnlyList<string> UpcomingReminderSummaries);
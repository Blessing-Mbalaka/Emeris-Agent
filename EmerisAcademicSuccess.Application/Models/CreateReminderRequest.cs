namespace EmerisAcademicSuccess.Application.Models;

public sealed record CreateReminderRequest(
    string Title,
    DateTime DueAtLocal,
    string? Notes);
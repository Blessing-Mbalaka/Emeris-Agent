namespace EmerisAcademicSuccess.Domain.Entities;

public sealed class Reminder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime DueAtLocal { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsCompleted { get; set; }
}
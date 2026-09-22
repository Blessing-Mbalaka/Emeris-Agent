namespace EmerisAcademicSuccess.Web.Models;

public sealed class ReminderFormModel
{
    public string Title { get; set; } = string.Empty;
    public DateTime? DueAtLocal { get; set; }
    public string? Notes { get; set; }
}
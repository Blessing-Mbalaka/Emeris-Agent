using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Web.Models;

public sealed class DashboardViewModel
{
    public string? StatusMessage { get; set; }
    public bool GeminiApiKeyAvailable { get; set; }
    public string GeminiApiKeySource { get; set; } = "Not configured";
    public string PersistenceMode { get; set; } = "InMemory";
    public IReadOnlyList<Reminder> UpcomingReminders { get; set; } = Array.Empty<Reminder>();
    public IReadOnlyList<DocumentAsset> UploadedDocuments { get; set; } = Array.Empty<DocumentAsset>();
    public IReadOnlyList<ScheduleEntry> ScheduleEntries { get; set; } = Array.Empty<ScheduleEntry>();
}
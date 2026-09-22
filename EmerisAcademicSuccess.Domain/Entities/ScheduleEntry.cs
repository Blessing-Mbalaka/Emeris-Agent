namespace EmerisAcademicSuccess.Domain.Entities;

public sealed class ScheduleEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentAssetId { get; set; }
    public DocumentAsset DocumentAsset { get; set; } = null!;
    public ScheduleEntryType EntryType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ModuleCode { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public DateOnly? OccursOn { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Venue { get; set; }
    public string? Notes { get; set; }
}
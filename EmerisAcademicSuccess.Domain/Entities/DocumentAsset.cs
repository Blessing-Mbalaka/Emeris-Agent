namespace EmerisAcademicSuccess.Domain.Entities;

public sealed class DocumentAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DocumentCategory Category { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
}
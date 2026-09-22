namespace EmerisAcademicSuccess.Domain.Entities;

public sealed class DocumentChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentAssetId { get; set; }
    public DocumentAsset DocumentAsset { get; set; } = null!;
    public int Sequence { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? EmbeddingJson { get; set; }
}
using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Application.Models;

public sealed record DocumentIngestionResult(
    DocumentAsset Document,
    int ChunkCount,
    int ExtractedScheduleItems);
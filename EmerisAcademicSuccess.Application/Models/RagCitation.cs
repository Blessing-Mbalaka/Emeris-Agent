using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Application.Models;

public sealed record RagCitation(
    Guid DocumentId,
    string DocumentName,
    DocumentCategory Category,
    string Excerpt,
    double Score);
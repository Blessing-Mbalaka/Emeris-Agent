using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IDocumentIngestionService
{
    Task<DocumentIngestionResult> IngestAsync(DocumentUploadRequest request, string? apiKey, CancellationToken cancellationToken);
}
using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IDocumentRepository
{
    Task AddAsync(DocumentAsset document, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentAsset>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentChunk>> GetChunksAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ScheduleEntry>> GetScheduleEntriesAsync(CancellationToken cancellationToken);
}
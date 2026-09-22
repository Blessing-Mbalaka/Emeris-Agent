using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmerisAcademicSuccess.Infrastructure.Persistence.Repositories;

public sealed class DocumentRepository(AcademicSuccessDbContext dbContext) : IDocumentRepository
{
    public async Task AddAsync(DocumentAsset document, CancellationToken cancellationToken)
    {
        dbContext.Documents.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentAsset>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Documents
            .Include(item => item.ScheduleEntries)
            .OrderByDescending(item => item.UploadedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentChunk>> GetChunksAsync(CancellationToken cancellationToken)
    {
        return await dbContext.DocumentChunks
            .Include(item => item.DocumentAsset)
            .OrderBy(item => item.DocumentAsset.UploadedAtUtc)
            .ThenBy(item => item.Sequence)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleEntry>> GetScheduleEntriesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ScheduleEntries
            .Include(item => item.DocumentAsset)
            .OrderBy(item => item.OccursOn)
            .ThenBy(item => item.DayOfWeek)
            .ThenBy(item => item.StartTime)
            .ToListAsync(cancellationToken);
    }
}
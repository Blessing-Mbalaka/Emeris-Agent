using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IRagService
{
    Task<RagSearchResult> SearchAsync(string query, int maxResults, string? apiKey, CancellationToken cancellationToken);
}
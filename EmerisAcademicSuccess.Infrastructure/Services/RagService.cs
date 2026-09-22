using System.Text.Json;
using System.Text.RegularExpressions;
using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class RagService(
    IDocumentRepository documentRepository,
    IGenerativeAiService generativeAiService) : IRagService
{
    public async Task<RagSearchResult> SearchAsync(string query, int maxResults, string? apiKey, CancellationToken cancellationToken)
    {
        var chunks = await documentRepository.GetChunksAsync(cancellationToken);
        if (chunks.Count == 0)
        {
            return new RagSearchResult("empty", Array.Empty<RagCitation>());
        }

        IReadOnlyList<float>? queryEmbedding = null;
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            queryEmbedding = await generativeAiService.CreateEmbeddingAsync(query, apiKey!, cancellationToken);
        }

        var ranked = chunks
            .Select(chunk =>
            {
                var lexicalScore = ComputeLexicalScore(query, chunk.Content);
                var embeddingScore = queryEmbedding is null || string.IsNullOrWhiteSpace(chunk.EmbeddingJson)
                    ? 0d
                    : CosineSimilarity(queryEmbedding, DeserializeEmbedding(chunk.EmbeddingJson!));
                var score = queryEmbedding is null ? lexicalScore : (embeddingScore * 0.7d) + (lexicalScore * 0.3d);
                return new { chunk, score };
            })
            .Where(item => item.score > 0)
            .OrderByDescending(item => item.score)
            .Take(maxResults)
            .Select(item => new RagCitation(
                item.chunk.DocumentAssetId,
                item.chunk.DocumentAsset.FileName,
                item.chunk.DocumentAsset.Category,
                Truncate(item.chunk.Content, 380),
                Math.Round(item.score, 3)))
            .ToList();

        var mode = queryEmbedding is null ? "keyword" : "hybrid";
        return new RagSearchResult(mode, ranked);
    }

    private static double ComputeLexicalScore(string query, string content)
    {
        var queryTerms = Tokenize(query).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (queryTerms.Count == 0)
        {
            return 0;
        }

        var contentTerms = Tokenize(content).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (contentTerms.Count == 0)
        {
            return 0;
        }

        var hits = queryTerms.Count(term => contentTerms.Contains(term));
        return (double)hits / queryTerms.Count;
    }

    private static IEnumerable<string> Tokenize(string value)
    {
        return Regex.Matches(value.ToLowerInvariant(), @"[a-z0-9]{3,}")
            .Select(match => match.Value);
    }

    private static IReadOnlyList<float> DeserializeEmbedding(string json)
    {
        return JsonSerializer.Deserialize<float[]>(json) ?? Array.Empty<float>();
    }

    private static double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        if (left.Count == 0 || left.Count != right.Count)
        {
            return 0;
        }

        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;

        for (var index = 0; index < left.Count; index++)
        {
            dot += left[index] * right[index];
            leftNorm += left[index] * left[index];
            rightNorm += right[index] * right[index];
        }

        if (leftNorm == 0 || rightNorm == 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm));
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }
}
using System.Text.Json;
using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Domain.Entities;
using Microsoft.Extensions.Hosting;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class DocumentIngestionService(
    IDocumentRepository documentRepository,
    IPdfTextExtractor pdfTextExtractor,
    ITextChunker textChunker,
    IGenerativeAiService generativeAiService,
    ScheduleHeuristicParser scheduleParser,
    IHostEnvironment environment,
    ISystemClock clock) : IDocumentIngestionService
{
    public async Task<DocumentIngestionResult> IngestAsync(DocumentUploadRequest request, string? apiKey, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(request.FileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only PDF uploads are supported in this version.");
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var uploadsPath = Path.Combine(environment.ContentRootPath, "App_Data", "Uploads");
        Directory.CreateDirectory(uploadsPath);

        var storedPath = Path.Combine(uploadsPath, storedFileName);
        await File.WriteAllBytesAsync(storedPath, request.ContentBytes, cancellationToken);

        var extractedText = await pdfTextExtractor.ExtractTextAsync(request.ContentBytes, cancellationToken);
        var chunks = textChunker.Chunk(extractedText);
        var scheduleEntries = scheduleParser.Parse(request.Category, extractedText);

        var document = new DocumentAsset
        {
            FileName = request.FileName,
            StoredFileName = storedFileName,
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/pdf" : request.ContentType,
            Category = request.Category,
            UploadedAtUtc = clock.UtcNow,
            ExtractedText = extractedText,
            Summary = scheduleParser.BuildSummary(request.Category, scheduleEntries, extractedText)
        };

        foreach (var chunk in chunks.Select((content, index) => new { content, index }))
        {
            IReadOnlyList<float>? embedding = null;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                embedding = await generativeAiService.CreateEmbeddingAsync(chunk.content, apiKey!, cancellationToken);
            }

            document.Chunks.Add(new DocumentChunk
            {
                Sequence = chunk.index,
                Content = chunk.content,
                EmbeddingJson = embedding is null ? null : JsonSerializer.Serialize(embedding)
            });
        }

        foreach (var entry in scheduleEntries)
        {
            document.ScheduleEntries.Add(entry);
        }

        await documentRepository.AddAsync(document, cancellationToken);

        return new DocumentIngestionResult(document, document.Chunks.Count, document.ScheduleEntries.Count);
    }
}
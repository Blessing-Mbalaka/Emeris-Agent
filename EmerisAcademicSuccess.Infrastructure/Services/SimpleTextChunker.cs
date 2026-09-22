using System.Text;
using EmerisAcademicSuccess.Application.Interfaces;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class SimpleTextChunker : ITextChunker
{
    public IReadOnlyList<string> Chunk(string text, int maxChunkLength = 900, int overlap = 120)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var normalized = text.Replace("\r", string.Empty);
        var paragraphs = normalized.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var chunks = new List<string>();
        var builder = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length >= maxChunkLength)
            {
                FlushChunk(builder, chunks, overlap);

                for (var index = 0; index < paragraph.Length; index += Math.Max(1, maxChunkLength - overlap))
                {
                    var length = Math.Min(maxChunkLength, paragraph.Length - index);
                    chunks.Add(paragraph.Substring(index, length).Trim());
                }

                continue;
            }

            if (builder.Length + paragraph.Length + 2 > maxChunkLength)
            {
                FlushChunk(builder, chunks, overlap);
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
            }

            builder.Append(paragraph.Trim());
        }

        if (builder.Length > 0)
        {
            chunks.Add(builder.ToString().Trim());
        }

        return chunks;
    }

    private static void FlushChunk(StringBuilder builder, List<string> chunks, int overlap)
    {
        if (builder.Length == 0)
        {
            return;
        }

        var chunk = builder.ToString().Trim();
        if (chunk.Length > 0)
        {
            chunks.Add(chunk);
        }

        var tail = chunk.Length <= overlap
            ? chunk
            : chunk[^overlap..];

        builder.Clear();
        if (tail.Length > 0)
        {
            builder.Append(tail);
        }
    }
}
namespace EmerisAcademicSuccess.Application.Interfaces;

public interface ITextChunker
{
    IReadOnlyList<string> Chunk(string text, int maxChunkLength = 900, int overlap = 120);
}
namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IPdfTextExtractor
{
    Task<string> ExtractTextAsync(byte[] contentBytes, CancellationToken cancellationToken);
}
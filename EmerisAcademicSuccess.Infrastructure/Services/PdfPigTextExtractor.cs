using EmerisAcademicSuccess.Application.Interfaces;
using UglyToad.PdfPig;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    public Task<string> ExtractTextAsync(byte[] contentBytes, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var stream = new MemoryStream(contentBytes);
        using var document = PdfDocument.Open(stream);
        var text = string.Join(
            Environment.NewLine + Environment.NewLine,
            document.GetPages().Select(page => page.Text));

        return Task.FromResult(text.Trim());
    }
}
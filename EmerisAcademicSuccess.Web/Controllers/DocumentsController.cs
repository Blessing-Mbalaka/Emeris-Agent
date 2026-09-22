using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Web.Models;
using EmerisAcademicSuccess.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmerisAcademicSuccess.Web.Controllers;

public sealed class DocumentsController(IDocumentIngestionService documentIngestionService, IGeminiApiKeyAccessor apiKeyAccessor) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DocumentUploadFormModel model, CancellationToken cancellationToken)
    {
        if (model.File is null || model.File.Length == 0)
        {
            TempData["StatusMessage"] = "Select a PDF before uploading.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            await using var memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream, cancellationToken);

            var result = await documentIngestionService.IngestAsync(
                new DocumentUploadRequest(model.File.FileName, model.File.ContentType, model.Category, memoryStream.ToArray()),
                apiKeyAccessor.GetActiveApiKey(),
                cancellationToken);

            TempData["StatusMessage"] = $"Uploaded {result.Document.FileName} and indexed {result.ChunkCount} chunks with {result.ExtractedScheduleItems} extracted schedule rows.";
        }
        catch (Exception exception)
        {
            TempData["StatusMessage"] = $"Upload failed: {exception.Message}";
        }

        return RedirectToAction("Index", "Home");
    }
}
using System.Diagnostics;
using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Infrastructure.Options;
using EmerisAcademicSuccess.Web.Models;
using EmerisAcademicSuccess.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EmerisAcademicSuccess.Web.Controllers;

public class HomeController(
    IDocumentRepository documentRepository,
    IReminderService reminderService,
    IGeminiApiKeyAccessor apiKeyAccessor,
    IConfiguration configuration,
    IOptions<GeminiOptions> geminiOptions) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var documents = await documentRepository.GetAllAsync(cancellationToken);
        var reminders = await reminderService.GetUpcomingAsync(8, cancellationToken);
        var scheduleEntries = await documentRepository.GetScheduleEntriesAsync(cancellationToken);

        var model = new DashboardViewModel
        {
            StatusMessage = TempData["StatusMessage"] as string,
            GeminiApiKeyAvailable = apiKeyAccessor.HasActiveApiKey(),
            GeminiApiKeySource = apiKeyAccessor.HasSessionApiKey ? "Session override" : (!string.IsNullOrWhiteSpace(geminiOptions.Value.ApiKey) ? "Configuration" : "Not configured"),
            PersistenceMode = configuration["Persistence:Provider"] ?? "InMemory",
            UpcomingReminders = reminders,
            UploadedDocuments = documents.Take(8).ToList(),
            ScheduleEntries = scheduleEntries.Take(12).ToList()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

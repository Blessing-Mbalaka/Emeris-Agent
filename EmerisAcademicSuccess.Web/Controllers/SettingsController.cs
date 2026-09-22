using EmerisAcademicSuccess.Web.Models;
using EmerisAcademicSuccess.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmerisAcademicSuccess.Web.Controllers;

public sealed class SettingsController(IGeminiApiKeyAccessor apiKeyAccessor) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveGeminiKey(GeminiSettingsFormModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ApiKey))
        {
            apiKeyAccessor.ClearSessionApiKey();
            TempData["StatusMessage"] = "Session Gemini API key cleared. The app will fall back to configuration if present.";
            return RedirectToAction("Index", "Home");
        }

        apiKeyAccessor.SetSessionApiKey(model.ApiKey.Trim());
        TempData["StatusMessage"] = "Gemini API key stored for the current session.";
        return RedirectToAction("Index", "Home");
    }
}
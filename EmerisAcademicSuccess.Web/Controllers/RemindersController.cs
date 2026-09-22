using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmerisAcademicSuccess.Web.Controllers;

public sealed class RemindersController(IReminderService reminderService) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReminderFormModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Title) || model.DueAtLocal is null)
        {
            TempData["StatusMessage"] = "Reminder title and due date are required.";
            return RedirectToAction("Index", "Home");
        }

        await reminderService.CreateAsync(new CreateReminderRequest(model.Title, model.DueAtLocal.Value, model.Notes), cancellationToken);
        TempData["StatusMessage"] = $"Reminder created for {model.Title}.";
        return RedirectToAction("Index", "Home");
    }
}
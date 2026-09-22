using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Web.Models;
using EmerisAcademicSuccess.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmerisAcademicSuccess.Web.Controllers;

[Route("agent")]
public sealed class AgentController(IAgentOrchestrator agentOrchestrator, IGeminiApiKeyAccessor apiKeyAccessor) : Controller
{
    [HttpPost("ask")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Ask([FromBody] AgentPromptInputModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Message))
        {
            return BadRequest(new { error = "Message is required." });
        }

        var response = await agentOrchestrator.ExecuteAsync(
            new AgentRequest(model.Message.Trim(), apiKeyAccessor.GetActiveApiKey()),
            cancellationToken);

        return Json(new
        {
            response.FinalResponse,
            response.ReasoningSummary,
            response.Actions,
            response.Citations,
            response.SearchMode
        });
    }
}
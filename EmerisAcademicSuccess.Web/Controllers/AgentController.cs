using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Web.Models;
using EmerisAcademicSuccess.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmerisAcademicSuccess.Web.Controllers;

[Route("agent")]
public sealed class AgentController(IAgentOrchestrator agentOrchestrator, IGeminiApiKeyAccessor apiKeyAccessor) : Controller
{
    private const string ConversationSessionKey = "ConversationId";

    [HttpPost("ask")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Ask([FromBody] AgentPromptInputModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Message))
        {
            return BadRequest(new { error = "Message is required." });
        }

        var response = await agentOrchestrator.ExecuteAsync(
            new AgentRequest(model.Message.Trim(), apiKeyAccessor.GetActiveApiKey(), GetOrCreateConversationId()),
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

    private Guid GetOrCreateConversationId()
    {
        var existingValue = HttpContext.Session.GetString(ConversationSessionKey);
        if (Guid.TryParse(existingValue, out var conversationId))
        {
            return conversationId;
        }

        conversationId = Guid.NewGuid();
        HttpContext.Session.SetString(ConversationSessionKey, conversationId.ToString());
        return conversationId;
    }
}
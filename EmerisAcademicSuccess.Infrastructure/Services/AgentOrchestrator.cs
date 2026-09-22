using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class AgentOrchestrator(
    IEnumerable<IAgentTool> tools,
    IReminderService reminderService,
    IDocumentRepository documentRepository,
    IRagService ragService,
    IGenerativeAiService generativeAiService,
    ISystemClock clock,
    HeuristicAgentPlanner heuristicPlanner,
    IOptions<GeminiOptions> options) : IAgentOrchestrator
{
    private readonly Dictionary<string, IAgentTool> _tools = tools.ToDictionary(tool => tool.Name, StringComparer.OrdinalIgnoreCase);
    private readonly GeminiOptions _options = options.Value;

    public async Task<AgentResponse> ExecuteAsync(AgentRequest request, CancellationToken cancellationToken)
    {
        var activeApiKey = string.IsNullOrWhiteSpace(request.ApiKeyOverride) ? _options.ApiKey : request.ApiKeyOverride;
        var currentLocalTime = clock.LocalNow;
        var promptContext = await BuildPromptContextAsync(request.Message, currentLocalTime, cancellationToken);
        var plan = !string.IsNullOrWhiteSpace(activeApiKey)
            ? await generativeAiService.PlanAsync(promptContext, activeApiKey!, cancellationToken)
            : null;
        plan ??= heuristicPlanner.CreatePlan(request.Message, currentLocalTime);

        var executionContext = new AgentExecutionContext(request.Message, activeApiKey, currentLocalTime);
        var actions = new List<AgentActionResult>();

        foreach (var action in plan.Actions)
        {
            if (!_tools.TryGetValue(action.ToolName, out var tool))
            {
                actions.Add(new AgentActionResult(action.ToolName, false, "Tool is not registered."));
                continue;
            }

            actions.Add(await tool.ExecuteAsync(action.Arguments, executionContext, cancellationToken));
        }

        var ragNeeded = plan.NeedsRag || actions.Any(action => string.Equals(action.ToolName, "search_documents", StringComparison.OrdinalIgnoreCase));
        var ragResult = ragNeeded
            ? await ragService.SearchAsync(request.Message, 5, activeApiKey, cancellationToken)
            : new RagSearchResult("skipped", Array.Empty<RagCitation>());

        if (ragNeeded && actions.All(action => !string.Equals(action.ToolName, "search_documents", StringComparison.OrdinalIgnoreCase)))
        {
            actions.Add(new AgentActionResult("search_documents", true, $"Retrieved {ragResult.Matches.Count} relevant document chunks using {ragResult.SearchMode} search."));
        }

        var responseContext = new AgentResponseContext(request.Message, currentLocalTime, plan, actions, ragResult);
        var finalResponse = !string.IsNullOrWhiteSpace(activeApiKey)
            ? await generativeAiService.ComposeResponseAsync(responseContext, activeApiKey!, cancellationToken)
            : null;

        finalResponse ??= BuildFallbackResponse(responseContext);

        return new AgentResponse(finalResponse, plan.Reasoning, actions, ragResult.Matches, ragResult.SearchMode);
    }

    private async Task<AgentPromptContext> BuildPromptContextAsync(string message, DateTime currentLocalTime, CancellationToken cancellationToken)
    {
        var reminders = await reminderService.GetUpcomingAsync(5, cancellationToken);
        var documents = await documentRepository.GetAllAsync(cancellationToken);

        return new AgentPromptContext(
            message,
            currentLocalTime,
            _tools.Values.Select(tool => $"{tool.Name}: {tool.Description}").ToList(),
            documents.Select(document => document.FileName).ToList(),
            reminders.Select(reminder => $"{reminder.Title} at {reminder.DueAtLocal:g}").ToList());
    }

    private static string BuildFallbackResponse(AgentResponseContext context)
    {
        var sections = new List<string>();

        if (context.Actions.Count > 0)
        {
            sections.Add("Actions taken:\n" + string.Join("\n", context.Actions.Select(action => $"- {action.Message}")));
        }

        if (context.RagResult.Matches.Count > 0)
        {
            sections.Add("Relevant document context:\n" + string.Join("\n", context.RagResult.Matches.Select(match => $"- {match.DocumentName}: {match.Excerpt}")));
        }

        if (context.Plan.NeedsStudyPlan)
        {
            sections.Add("Suggested study plan:\n- Review your next assessment items first.\n- Block two focused sessions before each due date.\n- Use uploaded study guides to prepare practice questions after each class.");
        }

        if (sections.Count == 0)
        {
            sections.Add("No specific action was taken. Upload timetable, assessment, or study guide PDFs to give the agent document context.");
        }

        return string.Join("\n\n", sections);
    }
}
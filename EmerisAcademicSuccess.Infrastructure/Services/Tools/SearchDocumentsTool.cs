using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;

namespace EmerisAcademicSuccess.Infrastructure.Services.Tools;

public sealed class SearchDocumentsTool(IRagService ragService) : IAgentTool
{
    public string Name => "search_documents";

    public string Description => "Searches uploaded timetable, assessment, and study guide PDFs for relevant context.";

    public async Task<AgentActionResult> ExecuteAsync(IReadOnlyDictionary<string, string> arguments, AgentExecutionContext context, CancellationToken cancellationToken)
    {
        var query = arguments.TryGetValue("query", out var rawQuery) && !string.IsNullOrWhiteSpace(rawQuery)
            ? rawQuery
            : context.UserMessage;
        var result = await ragService.SearchAsync(query, 3, context.ApiKey, cancellationToken);
        return new AgentActionResult(Name, true, $"Retrieved {result.Matches.Count} relevant document chunks using {result.SearchMode} search.");
    }
}
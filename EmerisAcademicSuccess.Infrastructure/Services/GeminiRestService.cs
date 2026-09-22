using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class GeminiRestService(HttpClient httpClient, IOptions<GeminiOptions> options) : IGenerativeAiService
{
    private readonly GeminiOptions _options = options.Value;

    public async Task<AgentPlanResult?> PlanAsync(AgentPromptContext context, string apiKey, CancellationToken cancellationToken)
    {
                var systemPrompt = $@"You are the planning layer of a modular academic success AI agent.
Return strict JSON only with this shape:
{{
    ""intent"": ""short intent"",
    ""reasoning"": ""short reasoning"",
    ""needsRag"": true,
    ""needsStudyPlan"": false,
    ""actions"": [
        {{
            ""toolName"": ""get_current_date|create_reminder|list_reminders|search_documents|get_schedule_overview"",
            ""arguments"": {{ ""key"": ""value"" }}
        }}
    ]
}}
Only choose tools from this list:
{string.Join("\n", context.AvailableTools)}
Current local time: {context.CurrentLocalTime:yyyy-MM-dd HH:mm}
Uploaded documents: {string.Join(", ", context.UploadedDocuments.DefaultIfEmpty("none"))}
Upcoming reminders: {string.Join(" | ", context.UpcomingReminderSummaries.DefaultIfEmpty("none"))}";

        var response = await GenerateContentAsync(systemPrompt, context.UserMessage, apiKey, true, cancellationToken);
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        var json = ExtractJson(response);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var actions = root.TryGetProperty("actions", out var actionsNode)
                ? actionsNode.EnumerateArray()
                    .Select(action => new AgentActionRequest(
                        action.GetProperty("toolName").GetString() ?? string.Empty,
                        action.TryGetProperty("arguments", out var argsNode)
                            ? argsNode.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.GetString() ?? string.Empty)
                            : new Dictionary<string, string>()))
                    .Where(action => !string.IsNullOrWhiteSpace(action.ToolName))
                    .ToList()
                : new List<AgentActionRequest>();

            return new AgentPlanResult(
                root.TryGetProperty("intent", out var intentNode) ? intentNode.GetString() ?? "student_support" : "student_support",
                root.TryGetProperty("reasoning", out var reasoningNode) ? reasoningNode.GetString() ?? string.Empty : string.Empty,
                actions,
                root.TryGetProperty("needsRag", out var ragNode) && ragNode.ValueKind == JsonValueKind.True,
                root.TryGetProperty("needsStudyPlan", out var planNode) && planNode.ValueKind == JsonValueKind.True);
        }
        catch
        {
            return null;
        }
    }

    public Task<string?> ComposeResponseAsync(AgentResponseContext context, string apiKey, CancellationToken cancellationToken)
    {
        var systemPrompt = $"""
You are the response layer of an academic success assistant.
Be specific, grounded, and action-oriented.
Use the tool outcomes and retrieved context. If information is missing, say so directly.
If the user asked for a study plan, produce a short study plan with dated actions.
Do not invent citations beyond the supplied context.
Current local time: {context.CurrentLocalTime:dddd, dd MMM yyyy HH:mm}
""";

        var userPrompt = $"""
User request:
{context.UserMessage}

Planner reasoning:
{context.Plan.Reasoning}

Tool outcomes:
{string.Join("\n", context.Actions.Select(action => $"- {action.ToolName}: {action.Message}"))}

Retrieved context ({context.RagResult.SearchMode}):
{context.RagResult.BuildContextBlock()}
""";

        return GenerateContentAsync(systemPrompt, userPrompt, apiKey, false, cancellationToken);
    }

    public async Task<IReadOnlyList<float>?> CreateEmbeddingAsync(string text, string apiKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildEmbeddingUrl(apiKey))
        {
            Content = JsonContent.Create(new
            {
                model = $"models/{_options.EmbeddingModel}",
                content = new
                {
                    parts = new[]
                    {
                        new { text = text.Length > 7000 ? text[..7000] : text }
                    }
                }
            })
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (!document.RootElement.TryGetProperty("embedding", out var embeddingNode)
            || !embeddingNode.TryGetProperty("values", out var valuesNode))
        {
            return null;
        }

        return valuesNode.EnumerateArray().Select(node => node.GetSingle()).ToArray();
    }

    private async Task<string?> GenerateContentAsync(string systemPrompt, string userPrompt, string apiKey, bool jsonResponse, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildGenerateUrl(apiKey))
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = userPrompt } }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = jsonResponse ? "application/json" : "text/plain",
                    temperature = 0.3
                }
            }), Encoding.UTF8, "application/json")
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var parts = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts");

        return string.Concat(parts.EnumerateArray().Select(part => part.GetProperty("text").GetString()));
    }

    private string BuildGenerateUrl(string apiKey)
    {
        return $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={apiKey}";
    }

    private string BuildEmbeddingUrl(string apiKey)
    {
        return $"https://generativelanguage.googleapis.com/v1beta/models/{_options.EmbeddingModel}:embedContent?key={apiKey}";
    }

    private static string? ExtractJson(string input)
    {
        var trimmed = input.Trim();
        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var start = trimmed.IndexOf('{');
            var end = trimmed.LastIndexOf('}');
            return start >= 0 && end > start ? trimmed[start..(end + 1)] : null;
        }

        return trimmed;
    }
}
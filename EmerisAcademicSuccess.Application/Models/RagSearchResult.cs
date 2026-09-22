namespace EmerisAcademicSuccess.Application.Models;

public sealed record RagSearchResult(
    string SearchMode,
    IReadOnlyList<RagCitation> Matches)
{
    public string BuildContextBlock()
    {
        if (Matches.Count == 0)
        {
            return "No uploaded document context matched the request.";
        }

        return string.Join(
            "\n\n",
            Matches.Select((match, index) =>
                $"[{index + 1}] {match.DocumentName} ({match.Category})\n{match.Excerpt}"));
    }
}
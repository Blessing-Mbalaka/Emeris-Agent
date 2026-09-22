namespace EmerisAcademicSuccess.Web.Services;

public interface IGeminiApiKeyAccessor
{
    bool HasSessionApiKey { get; }
    bool HasActiveApiKey();
    string? GetActiveApiKey();
    void SetSessionApiKey(string apiKey);
    void ClearSessionApiKey();
}
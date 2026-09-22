using EmerisAcademicSuccess.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace EmerisAcademicSuccess.Web.Services;

public sealed class SessionGeminiApiKeyAccessor(IHttpContextAccessor httpContextAccessor, IOptions<GeminiOptions> options) : IGeminiApiKeyAccessor
{
    private const string SessionKey = "GeminiApiKey";
    private readonly GeminiOptions _options = options.Value;

    public bool HasSessionApiKey => !string.IsNullOrWhiteSpace(httpContextAccessor.HttpContext?.Session.GetString(SessionKey));

    public bool HasActiveApiKey()
    {
        return !string.IsNullOrWhiteSpace(GetActiveApiKey());
    }

    public string? GetActiveApiKey()
    {
        var sessionValue = httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
        return !string.IsNullOrWhiteSpace(sessionValue) ? sessionValue : _options.ApiKey;
    }

    public void SetSessionApiKey(string apiKey)
    {
        httpContextAccessor.HttpContext?.Session.SetString(SessionKey, apiKey);
    }

    public void ClearSessionApiKey()
    {
        httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
    }
}
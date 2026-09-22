namespace EmerisAcademicSuccess.Infrastructure.Options;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gemini-2.5-flash";
    public string EmbeddingModel { get; set; } = "text-embedding-004";
}
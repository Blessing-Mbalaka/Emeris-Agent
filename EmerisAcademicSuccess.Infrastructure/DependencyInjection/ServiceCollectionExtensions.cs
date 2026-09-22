using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Infrastructure.Options;
using EmerisAcademicSuccess.Infrastructure.Persistence;
using EmerisAcademicSuccess.Infrastructure.Persistence.Repositories;
using EmerisAcademicSuccess.Infrastructure.Services;
using EmerisAcademicSuccess.Infrastructure.Services.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmerisAcademicSuccess.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));

        var postgresConnection = configuration.GetConnectionString("Postgres");
        var provider = configuration["Persistence:Provider"] ?? "InMemory";

        services.AddDbContext<AcademicSuccessDbContext>(options =>
        {
            if (string.Equals(provider, "Postgres", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(postgresConnection))
            {
                options.UseNpgsql(postgresConnection);
                return;
            }

            options.UseInMemoryDatabase("EmerisAcademicSuccess");
        });

        services.AddHttpClient<IGenerativeAiService, GeminiRestService>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddScoped<ISystemClock, SystemClock>();
        services.AddScoped<IPdfTextExtractor, PdfPigTextExtractor>();
        services.AddSingleton<ITextChunker, SimpleTextChunker>();
        services.AddSingleton<ScheduleHeuristicParser>();
        services.AddSingleton<HeuristicAgentPlanner>();
        services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
        services.AddScoped<IRagService, RagService>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

        services.AddScoped<IAgentTool, GetCurrentDateTool>();
        services.AddScoped<IAgentTool, CreateReminderTool>();
        services.AddScoped<IAgentTool, ListRemindersTool>();
        services.AddScoped<IAgentTool, SearchDocumentsTool>();
        services.AddScoped<IAgentTool, GetScheduleOverviewTool>();

        return services;
    }
}
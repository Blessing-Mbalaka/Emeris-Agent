using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmerisAcademicSuccess.Infrastructure.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeAcademicSuccessDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AcademicSuccessDbContext>();
        if (dbContext.Database.IsInMemory())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
}
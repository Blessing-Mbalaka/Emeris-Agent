using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmerisAcademicSuccess.Infrastructure.Persistence.Repositories;

public sealed class ReminderRepository(AcademicSuccessDbContext dbContext) : IReminderRepository
{
    public async Task AddAsync(Reminder reminder, CancellationToken cancellationToken)
    {
        dbContext.Reminders.Add(reminder);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Reminder>> GetUpcomingAsync(int take, CancellationToken cancellationToken)
    {
        return await dbContext.Reminders
            .Where(item => !item.IsCompleted)
            .OrderBy(item => item.DueAtLocal)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
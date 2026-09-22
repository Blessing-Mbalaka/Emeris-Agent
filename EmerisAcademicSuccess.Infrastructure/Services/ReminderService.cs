using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class ReminderService(IReminderRepository repository, ISystemClock clock) : IReminderService
{
    public async Task<Reminder> CreateAsync(CreateReminderRequest request, CancellationToken cancellationToken)
    {
        var reminder = new Reminder
        {
            Title = request.Title.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            DueAtLocal = request.DueAtLocal,
            CreatedAtUtc = clock.UtcNow
        };

        await repository.AddAsync(reminder, cancellationToken);
        return reminder;
    }

    public Task<IReadOnlyList<Reminder>> GetUpcomingAsync(int take, CancellationToken cancellationToken)
    {
        return repository.GetUpcomingAsync(take, cancellationToken);
    }
}
using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IReminderRepository
{
    Task AddAsync(Reminder reminder, CancellationToken cancellationToken);
    Task<IReadOnlyList<Reminder>> GetUpcomingAsync(int take, CancellationToken cancellationToken);
}
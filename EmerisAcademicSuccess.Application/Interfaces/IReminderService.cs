using EmerisAcademicSuccess.Application.Models;
using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IReminderService
{
    Task<Reminder> CreateAsync(CreateReminderRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<Reminder>> GetUpcomingAsync(int take, CancellationToken cancellationToken);
}
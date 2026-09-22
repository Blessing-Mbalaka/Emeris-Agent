using EmerisAcademicSuccess.Domain.Entities;

namespace EmerisAcademicSuccess.Application.Interfaces;

public interface IConversationRepository
{
    Task EnsureConversationAsync(Guid conversationId, string title, CancellationToken cancellationToken);
    Task AddMessageAsync(Guid conversationId, string role, string content, CancellationToken cancellationToken);
}
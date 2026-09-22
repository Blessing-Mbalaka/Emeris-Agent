using EmerisAcademicSuccess.Application.Interfaces;
using EmerisAcademicSuccess.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmerisAcademicSuccess.Infrastructure.Persistence.Repositories;

public sealed class ConversationRepository(AcademicSuccessDbContext dbContext, ISystemClock clock) : IConversationRepository
{
    public async Task EnsureConversationAsync(Guid conversationId, string title, CancellationToken cancellationToken)
    {
        var conversation = await dbContext.Conversations.FindAsync([conversationId], cancellationToken);
        if (conversation is not null)
        {
            return;
        }

        var now = clock.UtcNow;
        dbContext.Conversations.Add(new Conversation
        {
            Id = conversationId,
            Title = title.Length <= 200 ? title : title[..200],
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddMessageAsync(Guid conversationId, string role, string content, CancellationToken cancellationToken)
    {
        var conversation = await dbContext.Conversations.FindAsync([conversationId], cancellationToken)
            ?? throw new InvalidOperationException("Conversation must exist before messages can be stored.");

        dbContext.ConversationMessages.Add(new ConversationMessage
        {
            ConversationId = conversationId,
            Role = role,
            Content = content.Length <= 8000 ? content : content[..8000],
            CreatedAtUtc = clock.UtcNow
        });
        conversation.UpdatedAtUtc = clock.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
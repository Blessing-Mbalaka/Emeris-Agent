namespace EmerisAcademicSuccess.Domain.Entities;

public sealed class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
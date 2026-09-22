using EmerisAcademicSuccess.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmerisAcademicSuccess.Infrastructure.Persistence;

public sealed class AcademicSuccessDbContext(DbContextOptions<AcademicSuccessDbContext> options) : DbContext(options)
{
    public DbSet<DocumentAsset> Documents => Set<DocumentAsset>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<ScheduleEntry> ScheduleEntries => Set<ScheduleEntry>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentAsset>(entity =>
        {
            entity.Property(item => item.FileName).HasMaxLength(260);
            entity.Property(item => item.StoredFileName).HasMaxLength(260);
            entity.Property(item => item.ContentType).HasMaxLength(128);
            entity.Property(item => item.Summary).HasMaxLength(4000);
            entity.HasMany(item => item.Chunks)
                .WithOne(item => item.DocumentAsset)
                .HasForeignKey(item => item.DocumentAssetId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(item => item.ScheduleEntries)
                .WithOne(item => item.DocumentAsset)
                .HasForeignKey(item => item.DocumentAssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.Property(item => item.Content).HasMaxLength(4000);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.Property(item => item.Title).HasMaxLength(200);
            entity.HasMany(item => item.Messages)
                .WithOne(item => item.Conversation)
                .HasForeignKey(item => item.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConversationMessage>(entity =>
        {
            entity.Property(item => item.Role).HasMaxLength(32);
            entity.Property(item => item.Content).HasMaxLength(8000);
            entity.HasIndex(item => new { item.ConversationId, item.CreatedAtUtc });
        });

        modelBuilder.Entity<ScheduleEntry>(entity =>
        {
            entity.Property(item => item.Title).HasMaxLength(256);
            entity.Property(item => item.ModuleCode).HasMaxLength(64);
            entity.Property(item => item.Venue).HasMaxLength(256);
            entity.Property(item => item.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.Property(item => item.Title).HasMaxLength(200);
            entity.Property(item => item.Notes).HasMaxLength(1000);
        });
    }
}
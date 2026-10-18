using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    private const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks", t =>
        {
            var statuses = string.Join(", ", Enum.GetNames<TaskItemStatus>().Select(s => $"'{s}'"));
            t.HasCheckConstraint("ck_tasks_status", $"status IN ({statuses})");
            t.HasCheckConstraint("ck_tasks_completed_at", $"(status = '{nameof(TaskItemStatus.Completed)}') = (completed_at IS NOT NULL)");
            t.HasCheckConstraint("ck_tasks_due_after_start", "due_at IS NULL OR planned_start_at IS NULL OR due_at >= planned_start_at");
            t.HasCheckConstraint("ck_tasks_not_self_assigned", "assignee_id <> assigned_by_id");
        });

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: SeedData.IdentityStart);

        builder.Property(t => t.Title).HasMaxLength(TaskItem.TitleMaxLength).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(TaskItem.DescriptionMaxLength);
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(StatusMaxLength)
            .IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.Property<uint>("Version").IsRowVersion();

        builder.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedBy)
            .WithMany()
            .HasForeignKey(t => t.AssignedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.AssigneeId, t.Status });
        builder.HasIndex(t => new { t.AssigneeId, t.DueAt });
        builder.HasIndex(t => t.AssignedById);

        builder.HasData(
            new
            {
                Id = 1,
                Title = "Fix section 1 electricity",
                Status = TaskItemStatus.New,
                AssigneeId = SeedData.EngineerId,
                AssignedById = SeedData.ManagerId,
                PlannedStartAt = Utc(2026, 10, 20),
                DueAt = Utc(2026, 10, 31),
                CreatedAt = SeedData.CreatedAt,
                UpdatedAt = SeedData.CreatedAt
            },
            new
            {
                Id = 2,
                Title = "Fix section 2 electricity",
                Status = TaskItemStatus.InProgress,
                AssigneeId = SeedData.EngineerId,
                AssignedById = SeedData.ManagerId,
                PlannedStartAt = Utc(2026, 10, 5),
                DueAt = Utc(2026, 10, 15),
                CreatedAt = SeedData.CreatedAt,
                UpdatedAt = Utc(2026, 10, 5)
            },
            new
            {
                Id = 3,
                Title = "Create a report",
                Description = "Create last month financial report.",
                Status = TaskItemStatus.Completed,
                AssigneeId = SeedData.ManagerId,
                AssignedById = SeedData.EngineerId,
                PlannedStartAt = Utc(2026, 10, 1),
                DueAt = Utc(2026, 10, 3),
                CompletedAt = Utc(2026, 10, 2),
                CreatedAt = SeedData.CreatedAt,
                UpdatedAt = Utc(2026, 10, 2)
            });
    }

    private static DateTime Utc(int year, int month, int day) => new(year, month, day, 0, 0, 0, DateTimeKind.Utc);
}
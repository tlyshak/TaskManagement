using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Domain.Entities;
public class TaskItem
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private TaskItem()
    {
    }

    public int Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }

    public int AssigneeId { get; private set; }
    public Employee Assignee { get; private set; } = null!;

    public int AssignedById { get; private set; }
    public Employee AssignedBy { get; private set; } = null!;

    public DateTime? PlannedStartAt { get; private set; }
    public DateTime? DueAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static TaskItem Create(
        string title,
        string? description,
        Employee assignee,
        Employee assignedBy,
        DateTime? plannedStartAt,
        DateTime? dueAt,
        DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(assignee);
        ArgumentNullException.ThrowIfNull(assignedBy);
        Verifier.Utc(utcNow, nameof(utcNow));
        Verifier.Utc(plannedStartAt, nameof(plannedStartAt));
        Verifier.Utc(dueAt, nameof(dueAt));

        if (assignee.Id == assignedBy.Id)
            throw new BusinessRuleException("A task cannot be assigned by an employee to themselves.");

        if (!assignee.IsActive)
            throw new BusinessRuleException($"Employee {assignee.Id} is inactive and cannot be assigned a new task.");

        if (!assignedBy.IsActive)
            throw new BusinessRuleException($"Employee {assignedBy.Id} is inactive and cannot assign tasks.");

        EnsureDueNotBeforeStart(plannedStartAt, dueAt);

        if (description is { Length: > DescriptionMaxLength })
            throw new ArgumentException($"Description must be at most {DescriptionMaxLength} characters.", nameof(description));

        return new TaskItem
        {
            Title = Verifier.NotBlank(title, TitleMaxLength, nameof(title)),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = TaskItemStatus.New,
            AssigneeId = assignee.Id,
            Assignee = assignee,
            AssignedById = assignedBy.Id,
            AssignedBy = assignedBy,
            PlannedStartAt = plannedStartAt,
            DueAt = dueAt,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void ChangeStatus(TaskItemStatus newStatus, DateTime utcNow, DateTime? completedAtUtc = null)
    {
        Verifier.Utc(utcNow, nameof(utcNow));
        Verifier.Utc(completedAtUtc, nameof(completedAtUtc));

        if (!Enum.IsDefined(newStatus))
            throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, "Unknown task status.");

        if (Status.IsFinal())
            throw new BusinessRuleException($"Task {Id} is {Status}; a final status cannot be changed.");

        if (newStatus == Status)
            throw new BusinessRuleException($"Task {Id} is already {Status}.");

        if (completedAtUtc is not null && newStatus != TaskItemStatus.Completed)
            throw new BusinessRuleException("CompletedAt can only be set when the status is Completed.");

        Status = newStatus;
        CompletedAt = newStatus == TaskItemStatus.Completed ? completedAtUtc ?? utcNow : null;
        UpdatedAt = utcNow;
    }

    private static void EnsureDueNotBeforeStart(DateTime? plannedStartAt, DateTime? dueAt)
    {
        if (plannedStartAt is not null && dueAt is not null && dueAt < plannedStartAt)
            throw new BusinessRuleException("DueAt cannot be earlier than PlannedStartAt.");
    }
}

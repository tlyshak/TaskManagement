namespace TaskManagement.Domain.Enums;

public enum TaskItemStatus
{
    New,
    InProgress,
    OnHold,
    Completed,
    Cancelled
}

public static class TaskItemStatusExtensions
{
    public static bool IsFinal(this TaskItemStatus status) =>
        status is TaskItemStatus.Completed or TaskItemStatus.Cancelled;
}

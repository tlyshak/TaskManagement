using TaskManagement.Domain.Enums;

namespace TaskManagement.UI.Formatting;

public static class TaskItemStatusText
{
    public static string ToDisplayText(this TaskItemStatus status) => status switch
    {
        TaskItemStatus.New => "New",
        TaskItemStatus.InProgress => "In progress",
        TaskItemStatus.OnHold => "On hold",
        TaskItemStatus.Completed => "Completed",
        TaskItemStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };
}
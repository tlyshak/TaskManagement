using TaskManagement.Domain.Enums;

namespace TaskManagement.Data.DTOs;

public record CreateTaskRequest(
    string Title,
    string? Description,
    int AssigneeId,
    int AssignedById,
    DateTime? PlannedStartAt,
    DateTime? DueAt);

public record TaskListItemDto(
    int Id,
    string Title,
    TaskItemStatus Status,
    DateTime? PlannedStartAt,
    DateTime? DueAt,
    DateTime? CompletedAt,
    int AssignedById,
    string AssignedByName,
    bool IsOverdue);

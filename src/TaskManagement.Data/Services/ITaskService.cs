using TaskManagement.Data.DTOs;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Data.Services;

public interface ITaskService
{
    /// <summary>Create new task.</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Assignee or assigner does not exist.</exception>
    /// <exception cref="Domain.Exceptions.BusinessRuleException">A business rule is violated.</exception>
    Task<int> CreateTaskAsync(CreateTaskRequest request, CancellationToken ct = default);

    /// <summary>Change status of task.</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Task does not exist.</exception>
    /// <exception cref="Domain.Exceptions.BusinessRuleException">The transition is not allowed.</exception>
    Task ChangeStatusAsync(
        int taskId,
        TaskItemStatus newStatus,
        DateTime? completedAtUtc = null,
        CancellationToken ct = default);

    /// <summary>Tasks of the assignee.</summary>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Assignee does not exist.</exception>
    Task<IReadOnlyList<TaskListItemDto>> GetTasksByAssigneeAsync(int assigneeId, CancellationToken ct = default);
}
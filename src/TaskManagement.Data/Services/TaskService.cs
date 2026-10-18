using Microsoft.EntityFrameworkCore;
using TaskManagement.Data.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Data.Services;

public class TaskService(TaskManagementDbContext db, TimeProvider timeProvider) : ITaskService
{
    public async Task<int> CreateTaskAsync(CreateTaskRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assignee = await FindEmployeeAsync(request.AssigneeId, ct);
        var assignedBy = request.AssignedById == request.AssigneeId
            ? assignee
            : await FindEmployeeAsync(request.AssignedById, ct);

        var task = TaskItem.Create(
            request.Title,
            request.Description,
            assignee,
            assignedBy,
            request.PlannedStartAt,
            request.DueAt,
            UtcNow);

        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);

        return task.Id;
    }

    public async Task ChangeStatusAsync(
        int taskId,
        TaskItemStatus newStatus,
        DateTime? completedAtUtc = null,
        CancellationToken ct = default)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, ct)
                   ?? throw new EntityNotFoundException(nameof(TaskItem), taskId);

        task.ChangeStatus(newStatus, UtcNow, completedAtUtc);

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TaskListItemDto>> GetTasksByAssigneeAsync(
        int assigneeId,
        CancellationToken ct = default)
    {
        if (!await db.Employees.AnyAsync(e => e.Id == assigneeId, ct))
            throw new EntityNotFoundException(nameof(Employee), assigneeId);

        var now = UtcNow;

        return await db.Tasks
            .AsNoTracking()
            .Where(t => t.AssigneeId == assigneeId)
            .OrderBy(t => t.DueAt == null)
            .ThenBy(t => t.DueAt)
            .ThenBy(t => t.Id)
            .Select(t => new TaskListItemDto(
                t.Id,
                t.Title,
                t.Status,
                t.PlannedStartAt,
                t.DueAt,
                t.CompletedAt,
                t.AssignedById,
                t.AssignedBy.FullName,
                t.DueAt != null
                    && t.DueAt < now
                    && t.Status != TaskItemStatus.Completed
                    && t.Status != TaskItemStatus.Cancelled))
            .ToListAsync(ct);
    }

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    private async Task<Employee> FindEmployeeAsync(int id, CancellationToken ct) =>
        await db.Employees.FirstOrDefaultAsync(e => e.Id == id, ct)
        ?? throw new EntityNotFoundException(nameof(Employee), id);
}

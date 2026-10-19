using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskManagement.Data;
using TaskManagement.Data.DTOs;
using TaskManagement.Data.Services;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Tests.Infrastructure;

namespace TaskManagement.Tests.Integration;

public class TaskServiceTests(PostgresDatabaseFixture fixture) : IClassFixture<PostgresDatabaseFixture>
{
    private static readonly DateTime Now = new(2026, 10, 17, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Migrate_SeedsEmployeesAndTasks()
    {
        await using var db = fixture.CreateContext();

        int[] seededEmployees = [SeedData.ManagerId, SeedData.EngineerId, SeedData.InactiveEngineerId];
        Assert.Equal(seededEmployees, await db.Employees.Where(e => seededEmployees.Contains(e.Id)).OrderBy(e => e.Id).Select(e => e.Id).ToListAsync());
        Assert.Equal([1, 2, 3], await db.Tasks.Where(t => t.Id < SeedData.IdentityStart).OrderBy(t => t.Id).Select(t => t.Id).ToListAsync());
    }

    [Fact]
    public async Task CreateTask_Valid_AppearsInAssigneeList()
    {
        await using var db = fixture.CreateContext();
        var service = new TaskService(db, new FixedTimeProvider(Now));

        var id = await service.CreateTaskAsync(new CreateTaskRequest(
            "Grounding study", null, SeedData.EngineerId, SeedData.ManagerId, Now, Now.AddDays(3)));

        var tasks = await service.GetTasksByAssigneeAsync(SeedData.EngineerId);
        var created = Assert.Single(tasks, t => t.Id == id);
        Assert.Equal(TaskItemStatus.New, created.Status);
        Assert.True(id >= SeedData.IdentityStart);
    }

    [Fact]
    public async Task CreateTask_InactiveAssignee_ThrowsAndSavesNothing()
    {
        await using var db = fixture.CreateContext();
        var service = new TaskService(db, new FixedTimeProvider(Now));
        const string title = "Task for inactive employee";

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateTaskAsync(new CreateTaskRequest(
            title, null, SeedData.InactiveEngineerId, SeedData.ManagerId, null, null)));

        Assert.False(await db.Tasks.AnyAsync(t => t.Title == title));
    }

    [Fact]
    public async Task CreateTask_UnknownAssignee_ThrowsNotFound()
    {
        await using var db = fixture.CreateContext();
        var service = new TaskService(db, new FixedTimeProvider(Now));

        var ex = await Assert.ThrowsAsync<EntityNotFoundException>(() => service.CreateTaskAsync(new CreateTaskRequest(
            "Title", null, 999_999, SeedData.ManagerId, null, null)));
        Assert.Equal(999_999, ex.Id);
    }

    [Fact]
    public async Task ChangeStatus_ToCompleted_PersistsStatusAndCompletedAt()
    {
        var id = await CreateTaskAsync("Status change");

        await using (var db = fixture.CreateContext())
            await new TaskService(db, new FixedTimeProvider(Now.AddHours(1))).ChangeStatusAsync(id, TaskItemStatus.Completed);

        await using var verify = fixture.CreateContext();
        var task = await verify.Tasks.SingleAsync(t => t.Id == id);
        Assert.Equal(TaskItemStatus.Completed, task.Status);
        Assert.Equal(Now.AddHours(1), task.CompletedAt);
    }

    [Fact]
    public async Task ChangeStatus_FromFinalStatus_ThrowsAndKeepsStoredStatus()
    {
        var id = await CreateTaskAsync("Final status");
        await using (var db = fixture.CreateContext())
            await new TaskService(db, new FixedTimeProvider(Now)).ChangeStatusAsync(id, TaskItemStatus.Cancelled);

        await using (var db = fixture.CreateContext())
        {
            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                new TaskService(db, new FixedTimeProvider(Now)).ChangeStatusAsync(id, TaskItemStatus.InProgress));
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(TaskItemStatus.Cancelled, (await verify.Tasks.SingleAsync(t => t.Id == id)).Status);
    }

    [Fact]
    public async Task ChangeStatus_UnknownTask_ThrowsNotFound()
    {
        await using var db = fixture.CreateContext();
        var service = new TaskService(db, new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<EntityNotFoundException>(() => service.ChangeStatusAsync(999_999, TaskItemStatus.InProgress));
    }

    [Fact]
    public async Task GetTasksByAssignee_UnknownEmployee_ThrowsNotFound()
    {
        await using var db = fixture.CreateContext();
        var service = new TaskService(db, new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetTasksByAssigneeAsync(999_999));
    }

    [Theory]
    [InlineData("ck_tasks_completed_at", "completed_at = now()")]
    [InlineData("ck_tasks_due_after_start", "planned_start_at = now(), due_at = now() - interval '1 day'")]
    [InlineData("ck_tasks_not_self_assigned", "assigned_by_id = assignee_id")]
    [InlineData("ck_tasks_status", "status = 'Unknown'")]
    public async Task Database_RejectsRowsViolatingCheckConstraint(string constraint, string assignment)
    {
        var id = await CreateTaskAsync($"Constraint {constraint}");
        await using var db = fixture.CreateContext();

        var ex = await Assert.ThrowsAsync<PostgresException>(() =>
            db.Database.ExecuteSqlRawAsync("UPDATE tasks SET " + assignment + " WHERE id = {0}", id));

        Assert.Equal(PostgresErrorCodes.CheckViolation, ex.SqlState);
        Assert.Equal(constraint, ex.ConstraintName);
    }

    [Fact]
    public async Task GetEmployees_ReturnsActiveAndInactiveOrderedByName()
    {
        await using var db = fixture.CreateContext();
        var service = new EmployeeService(db);

        var employees = await service.GetEmployeesAsync();

        Assert.Equal(employees.OrderBy(e => e.FullName).Select(e => e.Id), employees.Select(e => e.Id));
        Assert.Contains(employees, e => e.Id == SeedData.InactiveEngineerId && !e.IsActive);
        Assert.Contains(employees, e => e.Id == SeedData.ManagerId && e.IsActive);
    }

    private async Task<int> CreateTaskAsync(string title)
    {
        await using var db = fixture.CreateContext();
        return await new TaskService(db, new FixedTimeProvider(Now)).CreateTaskAsync(new CreateTaskRequest(
            title, null, SeedData.EngineerId, SeedData.ManagerId, null, null));
    }
}

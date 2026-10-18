using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Data.DTOs;
using TaskManagement.Data.Services;
using TaskManagement.Domain.Enums;
using TaskManagement.Tests.Infrastructure;

namespace TaskManagement.Tests.Integration;

public class TaskServiceTests(PostgresDatabaseFixture fixture) : IClassFixture<PostgresDatabaseFixture>
{
    private static readonly DateTime Now = new(2026, 10, 17, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Migrate_SeedsEmployeesAndTasks()
    {
        await using var db = fixture.CreateContext();

        Assert.Equal(3, await db.Employees.CountAsync());
        Assert.Equal(3, await db.Tasks.CountAsync());
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
    public async Task GetEmployees_ReturnsActiveAndInactiveOrderedByName()
    {
        await using var db = fixture.CreateContext();
        var service = new EmployeeService(db);

        var employees = await service.GetEmployeesAsync();

        Assert.Equal(employees.OrderBy(e => e.FullName).Select(e => e.Id), employees.Select(e => e.Id));
        Assert.Contains(employees, e => e.Id == SeedData.InactiveEngineerId && !e.IsActive);
        Assert.Contains(employees, e => e.Id == SeedData.ManagerId && e.IsActive);
    }
}

using System.Reflection;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Tests.Domain;

public class TaskItemTests
{
    private static readonly DateTime Now = new(2026, 10, 17, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_SameAssigneeAndAssigner_Throws()
    {
        var employee = NewEmployee(1);

        Assert.Throws<BusinessRuleException>(() =>
            TaskItem.Create("Title", null, employee, employee, null, null, Now));
    }

    [Fact]
    public void Create_InactiveAssignee_Throws()
    {
        var assignee = NewEmployee(1);
        assignee.Deactivate();

        Assert.Throws<BusinessRuleException>(() =>
            TaskItem.Create("Title", null, assignee, NewEmployee(2), null, null, Now));
    }

    [Fact]
    public void Create_InactiveAssigner_Throws()
    {
        var assignedBy = NewEmployee(2);
        assignedBy.Deactivate();

        Assert.Throws<BusinessRuleException>(() =>
            TaskItem.Create("Title", null, NewEmployee(1), assignedBy, null, null, Now));
    }

    [Fact]
    public void Create_Valid_StartsAsNewWithoutCompletedAt()
    {
        var task = TaskItem.Create("  Title  ", null, NewEmployee(1), NewEmployee(2), Now, Now, Now);

        Assert.Equal(TaskItemStatus.New, task.Status);
        Assert.Equal("Title", task.Title);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void Create_DueBeforePlannedStart_Throws()
    {
        Assert.Throws<BusinessRuleException>(() =>
            TaskItem.Create("Title", null, NewEmployee(1), NewEmployee(2), Now, Now.AddDays(-1), Now));
    }

    [Fact]
    public void ChangeStatus_ToCompleted_SetsCompletedAt()
    {
        var task = NewTask();

        task.ChangeStatus(TaskItemStatus.Completed, Now);

        Assert.Equal(TaskItemStatus.Completed, task.Status);
        Assert.Equal(Now, task.CompletedAt);
    }

    [Fact]
    public void ChangeStatus_ToCompletedWithExplicitDate_UsesThatDate()
    {
        var task = NewTask();
        var completedAt = Now.AddHours(-2);

        task.ChangeStatus(TaskItemStatus.Completed, Now, completedAt);

        Assert.Equal(completedAt, task.CompletedAt);
    }

    [Theory]
    [InlineData(TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.OnHold)]
    [InlineData(TaskItemStatus.Cancelled)]
    public void ChangeStatus_CompletedAtForOtherStatus_ThrowsAndKeepsState(TaskItemStatus status)
    {
        var task = NewTask();

        Assert.Throws<BusinessRuleException>(() => task.ChangeStatus(status, Now, Now));
        Assert.Equal(TaskItemStatus.New, task.Status);
        Assert.Null(task.CompletedAt);
    }

    [Theory]
    [InlineData(TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.OnHold)]
    [InlineData(TaskItemStatus.Cancelled)]
    public void ChangeStatus_ToNonCompleted_LeavesCompletedAtEmpty(TaskItemStatus status)
    {
        var task = NewTask();

        task.ChangeStatus(status, Now);

        Assert.Equal(status, task.Status);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_Throws()
    {
        var task = NewTask();

        Assert.Throws<BusinessRuleException>(() => task.ChangeStatus(TaskItemStatus.New, Now));
    }

    [Theory]
    [InlineData(TaskItemStatus.Completed)]
    [InlineData(TaskItemStatus.Cancelled)]
    public void ChangeStatus_FromFinalStatus_ThrowsAndKeepsState(TaskItemStatus finalStatus)
    {
        var task = NewTask();
        task.ChangeStatus(finalStatus, Now);
        var completedAt = task.CompletedAt;

        Assert.Throws<BusinessRuleException>(() => task.ChangeStatus(TaskItemStatus.InProgress, Now.AddHours(1)));
        Assert.Equal(finalStatus, task.Status);
        Assert.Equal(completedAt, task.CompletedAt);
    }

    private static TaskItem NewTask() =>
        TaskItem.Create("Title", null, NewEmployee(1), NewEmployee(2), null, null, Now);

    private static Employee NewEmployee(int id)
    {
        var employee = new Employee($"Employee {id}", $"e{id}@example.com", null, Now);
        typeof(Employee).GetProperty(nameof(Employee.Id), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(employee, id);
        return employee;
    }
}

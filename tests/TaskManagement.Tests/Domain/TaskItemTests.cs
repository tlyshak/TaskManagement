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

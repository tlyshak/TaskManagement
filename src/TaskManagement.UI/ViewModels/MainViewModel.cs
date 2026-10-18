using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using TaskManagement.Data.DTOs;
using TaskManagement.Data.Services;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;
using TaskManagement.UI.Services;

namespace TaskManagement.UI.ViewModels;

public partial class MainViewModel(ScopedServiceRunner runner) : ObservableObject
{
    public ObservableCollection<EmployeeListItemDto> Employees { get; } = [];
    public ObservableCollection<TaskListItemDto> Tasks { get; } = [];
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateTaskCommand))]
    private EmployeeListItemDto? _selectedEmployee;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeStatusCommand))]
    private TaskListItemDto? _selectedTask;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeStatusCommand))]
    private TaskItemStatus? _newStatus;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateTaskCommand))]
    private string _newTitle = string.Empty;

    [ObservableProperty]
    private string? _newDescription;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateTaskCommand))]
    private EmployeeListItemDto? _newAssignedBy;

    [ObservableProperty]
    private DateTime? _newPlannedStart;

    [ObservableProperty]
    private DateTime? _newDue;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _openCount;

    [ObservableProperty]
    private int _overdueCount;

    [RelayCommand]
    private async Task LoadAsync()
    {
        await ExecuteAsync(async () =>
        {
            var employees = await runner.RunAsync<IEmployeeService, IReadOnlyList<EmployeeListItemDto>>(
                s => s.GetEmployeesAsync());

            Employees.Clear();
            foreach (var employee in employees)
                Employees.Add(employee);

            SelectedEmployee = Employees.FirstOrDefault(e => e.IsActive) ?? Employees.FirstOrDefault();
        });
    }

    [RelayCommand(CanExecute = nameof(CanCreateTask))]
    private async Task CreateTaskAsync()
    {
        await ExecuteAsync(async () =>
        {
            var request = new CreateTaskRequest(
                NewTitle,
                NewDescription,
                SelectedEmployee!.Id,
                NewAssignedBy!.Id,
                StartOfLocalDayUtc(NewPlannedStart),
                EndOfLocalDayUtc(NewDue));

            var id = await runner.RunAsync<ITaskService, int>(s => s.CreateTaskAsync(request));

            NewTitle = string.Empty;
            NewDescription = null;
            NewPlannedStart = null;
            NewDue = null;

            await ReloadTasksAsync(id);
        });
    }

    [RelayCommand(CanExecute = nameof(CanChangeStatus))]
    private async Task ChangeStatusAsync()
    {
        var taskId = SelectedTask!.Id;
        var status = NewStatus!.Value;

        await ExecuteAsync(async () =>
        {
            await runner.RunAsync<ITaskService>(s => s.ChangeStatusAsync(taskId, status));
            await ReloadTasksAsync(taskId);
        });
    }

    [RelayCommand]
    private void DismissError() => ErrorMessage = null;

    partial void OnSelectedEmployeeChanged(EmployeeListItemDto? value)
    {
        Tasks.Clear();
        UpdateCounters();
        if (value is not null)
            _ = ExecuteAsync(() => ReloadTasksAsync(selectTaskId: null));
    }

    partial void OnSelectedTaskChanged(TaskListItemDto? value) => NewStatus = null;

    private bool CanCreateTask() =>
        SelectedEmployee is not null && NewAssignedBy is not null && !string.IsNullOrWhiteSpace(NewTitle);

    private bool CanChangeStatus() =>
        SelectedTask is not null && !SelectedTask.Status.IsFinal() && NewStatus is not null && NewStatus != SelectedTask.Status;

    private async Task ReloadTasksAsync(int? selectTaskId)
    {
        if (SelectedEmployee is null)
            return;

        var assigneeId = SelectedEmployee.Id;
        var tasks = await runner.RunAsync<ITaskService, IReadOnlyList<TaskListItemDto>>(
            s => s.GetTasksByAssigneeAsync(assigneeId));

        if (SelectedEmployee?.Id != assigneeId)
            return;

        Tasks.Clear();
        foreach (var task in tasks)
            Tasks.Add(task);

        UpdateCounters();
        SelectedTask = Tasks.FirstOrDefault(t => t.Id == selectTaskId);
    }

    private void UpdateCounters()
    {
        TotalCount = Tasks.Count;
        OpenCount = Tasks.Count(t => !t.Status.IsFinal());
        OverdueCount = Tasks.Count(t => t.IsOverdue);
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        ErrorMessage = null;
        try
        {
            await action();
        }
        catch (Exception ex) when (ex is BusinessRuleException or EntityNotFoundException or ArgumentException)
        {
            ErrorMessage = ex.Message;
        }
        catch (DbUpdateConcurrencyException)
        {
            ErrorMessage = "The task was changed by someone else. The list has been refreshed, please try again.";
            await ReloadTasksAsync(SelectedTask?.Id);
        }
        catch (Exception ex) when (ex is DbUpdateException or Npgsql.NpgsqlException or InvalidOperationException)
        {
            ErrorMessage = "Database error: " + (ex.InnerException?.Message ?? ex.Message)
                + " Check that PostgreSQL is running and migrations are applied (dotnet ef database update).";
        }
    }

    private static DateTime? StartOfLocalDayUtc(DateTime? date) =>
        date is null ? null : DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Local).ToUniversalTime();

    private static DateTime? EndOfLocalDayUtc(DateTime? date) =>
        date is null
            ? null
            : DateTime.SpecifyKind(date.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Local).ToUniversalTime();
}


using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using TaskManagement.Domain.Enums;
using TaskManagement.UI.Formatting;

namespace TaskManagement.UI.Controls;

public sealed record StatusOption(TaskItemStatus Status, string Text, bool IsCurrent, bool IsSelected)
{
    public override string ToString() => Text;
}

[TemplateVisualState(GroupName = AvailabilityStates, Name = NoTaskState)]
[TemplateVisualState(GroupName = AvailabilityStates, Name = EditableState)]
[TemplateVisualState(GroupName = AvailabilityStates, Name = LockedState)]
public class StatusSelector : Control
{
    private const string AvailabilityStates = "AvailabilityStates";
    private const string NoTaskState = "NoTask";
    private const string EditableState = "Editable";
    private const string LockedState = "Locked";

    public static readonly RoutedCommand SelectCommand = new(nameof(SelectCommand), typeof(StatusSelector));

    static StatusSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusSelector), new FrameworkPropertyMetadata(typeof(StatusSelector)));

        CommandManager.RegisterClassCommandBinding(
            typeof(StatusSelector),
            new CommandBinding(SelectCommand, OnSelectExecuted, OnSelectCanExecute));
    }

    public StatusSelector() => UpdateOptions();

    #region CurrentStatus

    public static readonly DependencyProperty CurrentStatusProperty = DependencyProperty.Register(
        nameof(CurrentStatus),
        typeof(TaskItemStatus?),
        typeof(StatusSelector),
        new FrameworkPropertyMetadata(null, OnCurrentStatusChanged));

    public TaskItemStatus? CurrentStatus
    {
        get => (TaskItemStatus?)GetValue(CurrentStatusProperty);
        set => SetValue(CurrentStatusProperty, value);
    }

    private static void OnCurrentStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (StatusSelector)d;
        selector.SetValue(IsLockedPropertyKey, selector.CurrentStatus?.IsFinal() == true);

        selector.CoerceValue(SelectedStatusProperty);
        selector.UpdateOptions();
        selector.UpdateVisualState(useTransitions: true);
        CommandManager.InvalidateRequerySuggested();
    }

    #endregion

    #region SelectedStatus

    public static readonly DependencyProperty SelectedStatusProperty = DependencyProperty.Register(
        nameof(SelectedStatus),
        typeof(TaskItemStatus?),
        typeof(StatusSelector),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectedStatusChanged,
            CoerceSelectedStatus));

    public TaskItemStatus? SelectedStatus
    {
        get => (TaskItemStatus?)GetValue(SelectedStatusProperty);
        set => SetValue(SelectedStatusProperty, value);
    }

    private static object? CoerceSelectedStatus(DependencyObject d, object? baseValue) =>
        baseValue is TaskItemStatus status && !((StatusSelector)d).CanSelect(status) ? null : baseValue;

    private static void OnSelectedStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (StatusSelector)d;
        selector.UpdateOptions();
        selector.RaiseEvent(new RoutedPropertyChangedEventArgs<TaskItemStatus?>(
            (TaskItemStatus?)e.OldValue,
            (TaskItemStatus?)e.NewValue,
            SelectedStatusChangedEvent));
    }

    #endregion

    #region Read-only: Options, IsLocked

    private static readonly DependencyPropertyKey OptionsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Options),
        typeof(IReadOnlyList<StatusOption>),
        typeof(StatusSelector),
        new FrameworkPropertyMetadata(Array.Empty<StatusOption>()));

    public static readonly DependencyProperty OptionsProperty = OptionsPropertyKey.DependencyProperty;

    public IReadOnlyList<StatusOption> Options => (IReadOnlyList<StatusOption>)GetValue(OptionsProperty);

    private static readonly DependencyPropertyKey IsLockedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsLocked),
        typeof(bool),
        typeof(StatusSelector),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsLockedProperty = IsLockedPropertyKey.DependencyProperty;

    public bool IsLocked => (bool)GetValue(IsLockedProperty);

    #endregion

    #region SelectedStatusChanged routed event

    public static readonly RoutedEvent SelectedStatusChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectedStatusChanged),
        RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<TaskItemStatus?>),
        typeof(StatusSelector));

    public event RoutedPropertyChangedEventHandler<TaskItemStatus?> SelectedStatusChanged
    {
        add => AddHandler(SelectedStatusChangedEvent, value);
        remove => RemoveHandler(SelectedStatusChangedEvent, value);
    }

    #endregion

    public bool CanSelect(TaskItemStatus status) =>
        CurrentStatus is { } current && !current.IsFinal() && status != current;

    protected override AutomationPeer OnCreateAutomationPeer() => new StatusSelectorAutomationPeer(this);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateVisualState(useTransitions: false);
    }

    private static void OnSelectCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is StatusSelector selector && e.Parameter is TaskItemStatus status && selector.CanSelect(status);
        e.Handled = true;
    }

    private static void OnSelectExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is not StatusSelector selector || e.Parameter is not TaskItemStatus status)
            return;

        selector.SetCurrentValue(SelectedStatusProperty, status);
        e.Handled = true;
    }

    private void UpdateOptions() =>
        SetValue(OptionsPropertyKey, Enum.GetValues<TaskItemStatus>()
            .Select(s => new StatusOption(s, s.ToDisplayText(), s == CurrentStatus, s == SelectedStatus))
            .ToArray());

    private void UpdateVisualState(bool useTransitions)
    {
        var state = CurrentStatus is null ? NoTaskState
            : IsLocked ? LockedState
            : EditableState;

        VisualStateManager.GoToState(this, state, useTransitions);
    }
}

public class StatusSelectorAutomationPeer(StatusSelector owner) : FrameworkElementAutomationPeer(owner)
{
    protected override string GetClassNameCore() => nameof(StatusSelector);

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;

    protected override string GetNameCore() =>
        base.GetNameCore() is { Length: > 0 } name ? name : "Task status";
}
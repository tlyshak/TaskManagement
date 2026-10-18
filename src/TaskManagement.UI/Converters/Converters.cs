using System.Globalization;
using System.Windows.Data;
using TaskManagement.Domain.Enums;
using TaskManagement.UI.Formatting;

namespace TaskManagement.UI.Converters;

[ValueConversion(typeof(DateTime), typeof(DateTime))]
public sealed class UtcToLocalConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateTime utc ? DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime() : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

[ValueConversion(typeof(string), typeof(string))]
public sealed class InitialsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string name
            ? string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])))
            : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

[ValueConversion(typeof(TaskItemStatus), typeof(string))]
public sealed class StatusTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TaskItemStatus status ? status.ToDisplayText() : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
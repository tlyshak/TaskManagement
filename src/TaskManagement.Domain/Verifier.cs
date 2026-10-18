namespace TaskManagement.Domain;

internal static class Verifier
{
    public static DateTime Utc(DateTime value, string paramName) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : throw new ArgumentException("Date must be in UTC (DateTimeKind.Utc).", paramName);

    public static DateTime? Utc(DateTime? value, string paramName) =>
        value is null ? null : Utc(value.Value, paramName);

    public static string NotBlank(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value is required.", paramName);

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Value must be at most {maxLength} characters.", paramName);

        return trimmed;
    }
}
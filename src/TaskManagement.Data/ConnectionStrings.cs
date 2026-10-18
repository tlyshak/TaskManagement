namespace TaskManagement.Data;

public static class ConnectionStrings
{
    public const string EnvironmentVariable = "TASKDB_CONNECTION";

    public const string LocalDefault =
        "Host=localhost;Port=5432;Database=taskmanagement;Username=postgres;Password=postgres";

    public static string FromEnvironment() =>
        Environment.GetEnvironmentVariable(EnvironmentVariable) is { Length: > 0 } value
            ? value
            : LocalDefault;
}

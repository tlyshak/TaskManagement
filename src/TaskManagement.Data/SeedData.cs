namespace TaskManagement.Data;

public static class SeedData
{
    public static readonly DateTime CreatedAt = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    public const int ManagerId = 1;
    public const int EngineerId = 2;
    public const int InactiveEngineerId = 3;
    public const int IdentityStart = 4;
}

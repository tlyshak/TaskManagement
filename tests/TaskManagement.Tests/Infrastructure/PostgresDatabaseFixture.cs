using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskManagement.Data;

namespace TaskManagement.Tests.Infrastructure;

public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    public const string EnvironmentVariable = "TASKDB_TEST_CONNECTION";

    private readonly string _connectionString;

    public PostgresDatabaseFixture()
    {
        var baseConnection = Environment.GetEnvironmentVariable(EnvironmentVariable) is { Length: > 0 } value
            ? value
            : ConnectionStrings.LocalDefault;

        _connectionString = new NpgsqlConnectionStringBuilder(baseConnection)
        {
            Database = $"taskmanagement_test_{Guid.NewGuid():N}"
        }.ConnectionString;
    }

    public TaskManagementDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseTaskManagementDatabase(_connectionString)
            .Options);

    public async Task InitializeAsync()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateContext();
        await db.Database.EnsureDeletedAsync();
    }
}

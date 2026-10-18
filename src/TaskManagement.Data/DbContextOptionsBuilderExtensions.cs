using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Data;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder<TaskManagementDbContext> UseTaskManagementDatabase(
        this DbContextOptionsBuilder<TaskManagementDbContext> builder,
        string connectionString) =>
        builder
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention();
}
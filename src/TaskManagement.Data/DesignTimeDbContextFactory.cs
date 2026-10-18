using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManagement.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskManagementDbContext>
{
    public TaskManagementDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseTaskManagementDatabase(ConnectionStrings.FromEnvironment())
            .Options;

        return new TaskManagementDbContext(options);
    }
}

using Microsoft.EntityFrameworkCore;
using TaskManagement.Data.DTOs;

namespace TaskManagement.Data.Services;

public class EmployeeService(TaskManagementDbContext db) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeListItemDto>> GetEmployeesAsync(CancellationToken ct = default) =>
        await db.Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .ThenBy(e => e.Id)
            .Select(e => new EmployeeListItemDto(e.Id, e.FullName, e.Position, e.IsActive))
            .ToListAsync(ct);
}
using TaskManagement.Data.DTOs;

namespace TaskManagement.Data.Services;

public interface IEmployeeService
{
    /// <summary>Get all employees ordered by full name.</summary>
    Task<IReadOnlyList<EmployeeListItemDto>> GetEmployeesAsync(CancellationToken ct = default);
}
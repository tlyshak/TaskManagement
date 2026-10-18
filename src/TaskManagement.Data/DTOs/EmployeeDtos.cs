namespace TaskManagement.Data.DTOs;

public record EmployeeListItemDto(int Id, string FullName, string? Position, bool IsActive);

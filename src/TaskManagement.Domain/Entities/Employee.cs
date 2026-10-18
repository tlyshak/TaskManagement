namespace TaskManagement.Domain.Entities;

public class Employee
{
    public const int FullNameMaxLength = 200;
    public const int EmailMaxLength = 200;
    public const int PositionMaxLength = 100;

    private Employee()
    {
    }

    public Employee(string fullName, string email, string? position, DateTime createdAtUtc)
    {
        FullName = Verifier.NotBlank(fullName, FullNameMaxLength, nameof(fullName));
        Email = Verifier.NotBlank(email, EmailMaxLength, nameof(email));
        Position = position?.Trim();
        IsActive = true;
        CreatedAt = Verifier.Utc(createdAtUtc, nameof(createdAtUtc));
    }

    public int Id { get; private set; }
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Position { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: SeedData.IdentityStart);

        builder.Property(e => e.FullName).HasMaxLength(Employee.FullNameMaxLength).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(Employee.EmailMaxLength).IsRequired();
        builder.Property(e => e.Position).HasMaxLength(Employee.PositionMaxLength);
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.Email).IsUnique();

        builder.HasData(
            new
            {
                Id = SeedData.ManagerId,
                FullName = "Jack Sparrow",
                Email = "jack.sparrow@example.com",
                Position = "Project Manager",
                IsActive = true,
                SeedData.CreatedAt
            },
            new
            {
                Id = SeedData.EngineerId,
                FullName = "John Wick",
                Email = "john.wick@example.com",
                Position = "Electrical Engineer",
                IsActive = true,
                SeedData.CreatedAt
            },
            new
            {
                Id = SeedData.InactiveEngineerId,
                FullName = "Mickey Mouse",
                Email = "mickeymouse@example.com",
                Position = "Electrical Engineer",
                IsActive = false,
                SeedData.CreatedAt
            });
    }
}
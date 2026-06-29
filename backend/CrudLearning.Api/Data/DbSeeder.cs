using CrudLearning.Api.Models;
using CrudLearning.Api.Security;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, JwtSettings jwtSettings)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var employee = new Employee
        {
            FullName = "Ava Johnson",
            Email = "employee@crudlearning.local",
            Department = "Operations",
            Position = "Employee Specialist",
            WorkStatus = EmployeeWorkStatus.Working,
            AttendanceState = AttendanceState.CheckedOut,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        db.Users.AddRange(
            new AppUser
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 12),
                Role = UserRole.Admin,
                IsActive = true
            },
            new AppUser
            {
                Username = "employee",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee123!", workFactor: 12),
                Role = UserRole.Employee,
                IsActive = true,
                EmployeeId = employee.Id
            }
        );

        await db.SaveChangesAsync();
    }
}
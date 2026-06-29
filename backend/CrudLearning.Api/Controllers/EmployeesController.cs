using System.Security.Claims;
using BCrypt.Net;
using CrudLearning.Api.Contracts;
using CrudLearning.Api.Data;
using CrudLearning.Api.Models;
using CrudLearning.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmployeesController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeResponse>>> GetEmployees()
    {
        var employees = await _db.Employees
            .Include(employee => employee.UserAccount)
            .OrderBy(employee => employee.FullName)
            .Select(employee => ToResponse(employee))
            .ToListAsync();

        return Ok(employees);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<EmployeeResponse>> GetMe()
    {
        var employeeId = GetEmployeeIdFromToken();
        if (employeeId is null)
        {
            return NotFound();
        }

        var employee = await LoadEmployeeAsync(employeeId.Value);
        if (employee is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(employee));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeResponse>> GetEmployee(int id)
    {
        var employee = await LoadEmployeeAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(employee));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<EmployeeResponse>> CreateEmployee(CreateEmployeeRequest request)
    {
        if (await _db.Users.AnyAsync(user => user.Username.ToLower() == request.Username.Trim().ToLowerInvariant()))
        {
            return Conflict("Username already exists.");
        }

        var now = DateTime.UtcNow;
        var employee = new Employee
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Department = request.Department.Trim(),
            Position = request.Position.Trim(),
            WorkStatus = EmployeeWorkStatus.Working,
            AttendanceState = AttendanceState.CheckedOut,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        var user = new AppUser
        {
            Username = request.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            Role = UserRole.Employee,
            IsActive = true,
            EmployeeId = employee.Id
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        employee.UserAccount = user;

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, ToResponse(employee));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeRequest request)
    {
        var employee = await _db.Employees.Include(item => item.UserAccount).FirstOrDefaultAsync(item => item.Id == id);
        if (employee is null)
        {
            return NotFound();
        }

        employee.FullName = request.FullName.Trim();
        employee.Email = request.Email.Trim();
        employee.Department = request.Department.Trim();
        employee.Position = request.Position.Trim();
        employee.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDeleteEmployee(int id)
    {
        var employee = await _db.Employees.Include(item => item.UserAccount).FirstOrDefaultAsync(item => item.Id == id);
        if (employee is null)
        {
            return NotFound();
        }

        employee.IsDeleted = true;
        employee.UpdatedAt = DateTime.UtcNow;

        if (employee.UserAccount is not null)
        {
            employee.UserAccount.IsActive = false;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<EmployeeResponse>> UpdateStatus(int id, UpdateStatusRequest request)
    {
        var employee = await LoadEmployeeAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        if (!CanAccessEmployee(id))
        {
            return Forbid();
        }

        employee.WorkStatus = request.WorkStatus;
        employee.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(employee));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
    [HttpPost("{id:int}/check-in")]
    public async Task<ActionResult<EmployeeResponse>> CheckIn(int id)
    {
        var employee = await LoadEmployeeAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        if (!CanAccessEmployee(id))
        {
            return Forbid();
        }

        var now = DateTime.UtcNow;
        employee.AttendanceState = AttendanceState.CheckedIn;
        employee.WorkStatus = EmployeeWorkStatus.Working;
        employee.LastCheckInAt = now;
        employee.UpdatedAt = now;

        _db.AttendanceEntries.Add(new AttendanceEntry
        {
            EmployeeId = employee.Id,
            EventType = AttendanceEventType.CheckIn,
            OccurredAt = now
        });

        await _db.SaveChangesAsync();
        return Ok(ToResponse(employee));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
    [HttpPost("{id:int}/check-out")]
    public async Task<ActionResult<EmployeeResponse>> CheckOut(int id)
    {
        var employee = await LoadEmployeeAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        if (!CanAccessEmployee(id))
        {
            return Forbid();
        }

        var now = DateTime.UtcNow;
        employee.AttendanceState = AttendanceState.CheckedOut;
        employee.LastCheckOutAt = now;
        employee.UpdatedAt = now;

        _db.AttendanceEntries.Add(new AttendanceEntry
        {
            EmployeeId = employee.Id,
            EventType = AttendanceEventType.CheckOut,
            OccurredAt = now
        });

        await _db.SaveChangesAsync();
        return Ok(ToResponse(employee));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
    [HttpGet("{id:int}/attendance")]
    public async Task<ActionResult<IEnumerable<AttendanceEntryResponse>>> GetAttendance(int id)
    {
        if (!CanAccessEmployee(id))
        {
            return Forbid();
        }

        var entries = await _db.AttendanceEntries
            .Include(entry => entry.Employee)
            .Where(entry => entry.EmployeeId == id)
            .OrderByDescending(entry => entry.OccurredAt)
            .Take(20)
            .Select(entry => new AttendanceEntryResponse(
                entry.Id,
                entry.EmployeeId,
                entry.Employee.FullName,
                entry.EventType,
                entry.OccurredAt,
                entry.Note
            ))
            .ToListAsync();

        return Ok(entries);
    }

    private async Task<Employee?> LoadEmployeeAsync(int id)
    {
        return await _db.Employees
            .Include(employee => employee.UserAccount)
            .FirstOrDefaultAsync(employee => employee.Id == id);
    }

    private bool CanAccessEmployee(int employeeId)
    {
        if (User.IsInRole(RoleNames.Admin))
        {
            return true;
        }

        var tokenEmployeeId = GetEmployeeIdFromToken();
        return tokenEmployeeId.HasValue && tokenEmployeeId.Value == employeeId;
    }

    private int? GetEmployeeIdFromToken()
    {
        var employeeIdClaim = User.FindFirstValue("employee_id");
        if (string.IsNullOrWhiteSpace(employeeIdClaim))
        {
            return null;
        }

        return int.TryParse(employeeIdClaim, out var employeeId) ? employeeId : null;
    }

    private static EmployeeResponse ToResponse(Employee employee)
    {
        return new EmployeeResponse(
            employee.Id,
            employee.FullName,
            employee.Email,
            employee.Department,
            employee.Position,
            employee.UserAccount?.Username ?? string.Empty,
            employee.UserAccount?.Role ?? UserRole.Employee,
            employee.WorkStatus,
            employee.AttendanceState,
            employee.IsDeleted,
            employee.LastCheckInAt,
            employee.LastCheckOutAt,
            employee.CreatedAt,
            employee.UpdatedAt
        );
    }
}
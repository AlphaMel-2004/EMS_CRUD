using CrudLearning.Api.Data;
using CrudLearning.Api.DTOs.Attendance;
using CrudLearning.Api.DTOs.Employees;
using CrudLearning.Api.Middleware;
using CrudLearning.Api.Models;
using CrudLearning.Api.Services.Audit;
using CrudLearning.Api.Services.Employees;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Services.Attendance;

public sealed class AttendanceService
{
    private readonly AppDbContext _db;
    private readonly AuditLogService _auditLogService;

    public AttendanceService(AppDbContext db, AuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public async Task<EmployeeResponse> CheckInAsync(int employeeId, int? actorUserId)
    {
        var employee = await LoadEmployeeAsync(employeeId);
        if (employee.AttendanceState == AttendanceState.CheckedIn)
        {
            throw new ApiException("Employee is already checked in.", StatusCodes.Status409Conflict);
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
            OccurredAt = now,
            Note = "Checked in"
        });

        await _db.SaveChangesAsync();
        await _auditLogService.RecordAsync("EmployeeCheckedIn", actorUserId, employee.Id, "Employee checked in.");
        return EmployeeService.ToResponse(employee);
    }

    public async Task<EmployeeResponse> CheckOutAsync(int employeeId, int? actorUserId)
    {
        var employee = await LoadEmployeeAsync(employeeId);
        if (employee.AttendanceState == AttendanceState.CheckedOut)
        {
            throw new ApiException("Employee is already checked out.", StatusCodes.Status409Conflict);
        }

        var now = DateTime.UtcNow;
        employee.AttendanceState = AttendanceState.CheckedOut;
        employee.LastCheckOutAt = now;
        employee.UpdatedAt = now;

        _db.AttendanceEntries.Add(new AttendanceEntry
        {
            EmployeeId = employee.Id,
            EventType = AttendanceEventType.CheckOut,
            OccurredAt = now,
            Note = "Checked out"
        });

        await _db.SaveChangesAsync();
        await _auditLogService.RecordAsync("EmployeeCheckedOut", actorUserId, employee.Id, "Employee checked out.");
        return EmployeeService.ToResponse(employee);
    }

    public async Task<IReadOnlyList<AttendanceEntryResponse>> GetRecentAttendanceAsync(int employeeId)
    {
        var employeeExists = await _db.Employees.AnyAsync(employee => employee.Id == employeeId);
        if (!employeeExists)
        {
            throw new ApiException("Employee not found.", StatusCodes.Status404NotFound);
        }

        return await _db.AttendanceEntries
            .Include(entry => entry.Employee)
            .Where(entry => entry.EmployeeId == employeeId)
            .OrderByDescending(entry => entry.OccurredAt)
            .Take(20)
            .Select(entry => new AttendanceEntryResponse(
                entry.Id,
                entry.EmployeeId,
                entry.Employee.FullName,
                entry.EventType,
                entry.OccurredAt,
                entry.Note))
            .ToListAsync();
    }

    private async Task<Employee> LoadEmployeeAsync(int employeeId)
    {
        var employee = await _db.Employees
            .Include(item => item.UserAccount)
            .FirstOrDefaultAsync(item => item.Id == employeeId);

        if (employee is null)
        {
            throw new ApiException("Employee not found.", StatusCodes.Status404NotFound);
        }

        if (employee.IsDeleted)
        {
            throw new ApiException("Deleted employee records cannot be updated.", StatusCodes.Status400BadRequest);
        }

        return employee;
    }
}

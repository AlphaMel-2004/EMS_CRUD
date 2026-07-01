using BCrypt.Net;
using CrudLearning.Api.Data;
using CrudLearning.Api.DTOs.Employees;
using CrudLearning.Api.Helpers;
using CrudLearning.Api.Middleware;
using CrudLearning.Api.Models;
using CrudLearning.Api.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Services.Employees;

public sealed class EmployeeService
{
    private readonly AppDbContext _db;
    private readonly AuditLogService _auditLogService;

    public EmployeeService(AppDbContext db, AuditLogService auditLogService)
    {
        _db = db;
        _auditLogService = auditLogService;
    }

    public async Task<PaginatedResponse<EmployeeResponse>> GetEmployeesAsync(EmployeeQuery query)
    {
        var page = Pagination.NormalizePage(query.Page);
        var pageSize = Pagination.NormalizePageSize(query.PageSize);
        var employees = _db.Employees.Include(employee => employee.UserAccount).AsQueryable();

        if (!query.IncludeDeleted)
        {
            employees = employees.Where(employee => !employee.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            employees = employees.Where(employee =>
                employee.FullName.ToLower().Contains(search) ||
                employee.Email.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var department = query.Department.Trim().ToLowerInvariant();
            employees = employees.Where(employee => employee.Department.ToLower() == department);
        }

        if (query.WorkStatus is not null)
        {
            employees = employees.Where(employee => employee.WorkStatus == query.WorkStatus);
        }

        if (query.AttendanceState is not null)
        {
            employees = employees.Where(employee => employee.AttendanceState == query.AttendanceState);
        }

        var totalItems = await employees.CountAsync();
        var items = await employees
            .OrderBy(employee => employee.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(employee => ToResponse(employee))
            .ToListAsync();

        return new PaginatedResponse<EmployeeResponse>(
            items,
            page,
            pageSize,
            totalItems,
            Pagination.CountTotalPages(totalItems, pageSize));
    }

    public async Task<EmployeeResponse> GetEmployeeAsync(int id)
    {
        var employee = await LoadEmployeeAsync(id);
        return ToResponse(employee ?? throw new ApiException("Employee not found.", StatusCodes.Status404NotFound));
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request, int? actorUserId)
    {
        await EnsureUniqueUserAsync(request.Username, null);
        await EnsureUniqueEmailAsync(request.Email, null);
        EnsureStrongPassword(request.Password);

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
        await _auditLogService.RecordAsync("EmployeeCreated", actorUserId, employee.Id, $"Employee {employee.FullName} was created.");

        return ToResponse(employee);
    }

    public async Task UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, int? actorUserId)
    {
        var employee = await LoadEmployeeAsync(id)
            ?? throw new ApiException("Employee not found.", StatusCodes.Status404NotFound);

        await EnsureUniqueEmailAsync(request.Email, id);

        employee.FullName = request.FullName.Trim();
        employee.Email = request.Email.Trim();
        employee.Department = request.Department.Trim();
        employee.Position = request.Position.Trim();
        employee.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _auditLogService.RecordAsync("EmployeeUpdated", actorUserId, employee.Id, $"Employee {employee.FullName} was updated.");
    }

    public async Task SoftDeleteEmployeeAsync(int id, int? actorUserId)
    {
        var employee = await LoadEmployeeAsync(id)
            ?? throw new ApiException("Employee not found.", StatusCodes.Status404NotFound);

        employee.IsDeleted = true;
        employee.UpdatedAt = DateTime.UtcNow;

        if (employee.UserAccount is not null)
        {
            employee.UserAccount.IsActive = false;
        }

        await _db.SaveChangesAsync();
        await _auditLogService.RecordAsync("EmployeeSoftDeleted", actorUserId, employee.Id, $"Employee {employee.FullName} was soft deleted.");
    }

    public async Task<EmployeeResponse> UpdateStatusAsync(int id, EmployeeWorkStatus workStatus, int? actorUserId)
    {
        var employee = await LoadEmployeeAsync(id)
            ?? throw new ApiException("Employee not found.", StatusCodes.Status404NotFound);

        if (employee.IsDeleted)
        {
            throw new ApiException("Deleted employee records cannot be updated.", StatusCodes.Status400BadRequest);
        }

        employee.WorkStatus = workStatus;
        employee.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _auditLogService.RecordAsync("EmployeeStatusUpdated", actorUserId, employee.Id, $"Employee status changed to {workStatus}.");
        return ToResponse(employee);
    }

    private async Task<Employee?> LoadEmployeeAsync(int id)
    {
        return await _db.Employees
            .Include(employee => employee.UserAccount)
            .FirstOrDefaultAsync(employee => employee.Id == id);
    }

    private async Task EnsureUniqueUserAsync(string username, int? currentUserId)
    {
        var normalized = username.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(user =>
            user.Username.ToLower() == normalized && (currentUserId == null || user.Id != currentUserId));

        if (exists)
        {
            throw new ApiException("Username already exists.", StatusCodes.Status409Conflict);
        }
    }

    private async Task EnsureUniqueEmailAsync(string email, int? currentEmployeeId)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var exists = await _db.Employees.AnyAsync(employee =>
            employee.Email.ToLower() == normalized && (currentEmployeeId == null || employee.Id != currentEmployeeId));

        if (exists)
        {
            throw new ApiException("Email already exists.", StatusCodes.Status409Conflict);
        }
    }

    private static void EnsureStrongPassword(string password)
    {
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSymbol = password.Any(character => !char.IsLetterOrDigit(character));

        if (password.Length < 8 || !hasUpper || !hasLower || !hasDigit || !hasSymbol)
        {
            throw new ApiException("Password must be at least 8 characters and include uppercase, lowercase, number, and symbol characters.");
        }
    }

    public static EmployeeResponse ToResponse(Employee employee)
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
            employee.UpdatedAt);
    }
}

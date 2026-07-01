using CrudLearning.Api.DTOs.Attendance;
using CrudLearning.Api.DTOs.Employees;
using CrudLearning.Api.Helpers;
using CrudLearning.Api.Middleware;
using CrudLearning.Api.Security;
using CrudLearning.Api.Services.Attendance;
using CrudLearning.Api.Services.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudLearning.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;
    private readonly AttendanceService _attendanceService;

    public EmployeesController(EmployeeService employeeService, AttendanceService attendanceService)
    {
        _employeeService = employeeService;
        _attendanceService = attendanceService;
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<EmployeeResponse>>> GetEmployees([FromQuery] EmployeeQuery query)
    {
        return Ok(await _employeeService.GetEmployeesAsync(query));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<EmployeeResponse>> GetMe()
    {
        var employeeId = Permissions.GetEmployeeId(User);
        if (employeeId is null)
        {
            throw new ApiException("Employee not found.", StatusCodes.Status404NotFound);
        }

        return Ok(await _employeeService.GetEmployeeAsync(employeeId.Value));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeResponse>> GetEmployee(int id)
    {
        return Ok(await _employeeService.GetEmployeeAsync(id));
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult<EmployeeResponse>> CreateEmployee(CreateEmployeeRequest request)
    {
        var employee = await _employeeService.CreateEmployeeAsync(request, Permissions.GetUserId(User));
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeRequest request)
    {
        await _employeeService.UpdateEmployeeAsync(id, request, Permissions.GetUserId(User));
        return NoContent();
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> SoftDeleteEmployee(int id)
    {
        await _employeeService.SoftDeleteEmployeeAsync(id, Permissions.GetUserId(User));
        return NoContent();
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<EmployeeResponse>> UpdateStatus(int id, UpdateStatusRequest request)
    {
        EnsureEmployeeAccess(id);
        return Ok(await _employeeService.UpdateStatusAsync(id, request.WorkStatus, Permissions.GetUserId(User)));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
    [HttpPost("{id:int}/check-in")]
    public async Task<ActionResult<EmployeeResponse>> CheckIn(int id)
    {
        EnsureEmployeeAccess(id);
        return Ok(await _attendanceService.CheckInAsync(id, Permissions.GetUserId(User)));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
    [HttpPost("{id:int}/check-out")]
    public async Task<ActionResult<EmployeeResponse>> CheckOut(int id)
    {
        EnsureEmployeeAccess(id);
        return Ok(await _attendanceService.CheckOutAsync(id, Permissions.GetUserId(User)));
    }

    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Employee)]
    [HttpGet("{id:int}/attendance")]
    public async Task<ActionResult<IReadOnlyList<AttendanceEntryResponse>>> GetAttendance(int id)
    {
        EnsureEmployeeAccess(id);
        return Ok(await _attendanceService.GetRecentAttendanceAsync(id));
    }

    private void EnsureEmployeeAccess(int employeeId)
    {
        if (!Permissions.CanAccessEmployee(User, employeeId))
        {
            throw new ApiException("You are not allowed to perform this action.", StatusCodes.Status403Forbidden);
        }
    }
}

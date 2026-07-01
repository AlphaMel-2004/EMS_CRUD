using System.Text;
using CrudLearning.Api.DTOs.Reports;
using CrudLearning.Api.Security;
using CrudLearning.Api.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudLearning.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportsService _reportsService;

    public ReportsController(ReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    [HttpGet("daily")]
    public async Task<ActionResult<AttendanceSummaryResponse>> GetDailySummary(DateOnly? date)
    {
        return Ok(await _reportsService.GetDailySummaryAsync(date ?? DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [HttpGet("monthly")]
    public async Task<ActionResult<MonthlyAttendanceSummaryResponse>> GetMonthlySummary(int? year, int? month)
    {
        var now = DateTime.UtcNow;
        return Ok(await _reportsService.GetMonthlySummaryAsync(year ?? now.Year, month ?? now.Month));
    }

    [HttpGet("attendance.csv")]
    public async Task<FileContentResult> ExportAttendanceCsv(DateOnly? date)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var csv = await _reportsService.ExportAttendanceCsvAsync(reportDate);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"attendance-{reportDate:yyyy-MM-dd}.csv");
    }
}

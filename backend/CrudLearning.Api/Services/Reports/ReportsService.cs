using CrudLearning.Api.Data;
using CrudLearning.Api.DTOs.Reports;
using CrudLearning.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Services.Reports;

public sealed class ReportsService
{
    private readonly AppDbContext _db;

    public ReportsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AttendanceSummaryResponse> GetDailySummaryAsync(DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var activeEmployees = _db.Employees.Where(employee => !employee.IsDeleted);
        var totalEmployees = await activeEmployees.CountAsync();
        var absent = await activeEmployees.CountAsync(employee => employee.WorkStatus == EmployeeWorkStatus.Absent);
        var onLeave = await activeEmployees.CountAsync(employee => employee.WorkStatus == EmployeeWorkStatus.Leave);
        var checkedIn = await _db.AttendanceEntries.CountAsync(entry =>
            entry.EventType == AttendanceEventType.CheckIn &&
            entry.OccurredAt >= start &&
            entry.OccurredAt < end);
        var checkedOut = await _db.AttendanceEntries.CountAsync(entry =>
            entry.EventType == AttendanceEventType.CheckOut &&
            entry.OccurredAt >= start &&
            entry.OccurredAt < end);

        return new AttendanceSummaryResponse(date, totalEmployees, checkedIn, checkedOut, absent, onLeave);
    }

    public async Task<MonthlyAttendanceSummaryResponse> GetMonthlySummaryAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);
        var entries = _db.AttendanceEntries.Where(entry => entry.OccurredAt >= start && entry.OccurredAt < end);

        var checkIns = await entries.CountAsync(entry => entry.EventType == AttendanceEventType.CheckIn);
        var checkOuts = await entries.CountAsync(entry => entry.EventType == AttendanceEventType.CheckOut);
        var uniqueEmployees = await entries.Select(entry => entry.EmployeeId).Distinct().CountAsync();
        var weekdays = Enumerable.Range(0, DateTime.DaysInMonth(year, month))
            .Select(day => start.AddDays(day))
            .Count(day => day.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday);

        return new MonthlyAttendanceSummaryResponse(year, month, checkIns, checkOuts, uniqueEmployees, weekdays);
    }

    public async Task<string> ExportAttendanceCsvAsync(DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);
        var rows = await _db.AttendanceEntries
            .Include(entry => entry.Employee)
            .Where(entry => entry.OccurredAt >= start && entry.OccurredAt < end)
            .OrderBy(entry => entry.OccurredAt)
            .Select(entry => new
            {
                entry.EmployeeId,
                entry.Employee.FullName,
                entry.EventType,
                entry.OccurredAt,
                entry.Note
            })
            .ToListAsync();

        var lines = new List<string> { "EmployeeId,EmployeeName,EventType,OccurredAt,Note" };
        lines.AddRange(rows.Select(row =>
            $"{row.EmployeeId},\"{row.FullName.Replace("\"", "\"\"")}\",{row.EventType},{row.OccurredAt:o},\"{(row.Note ?? string.Empty).Replace("\"", "\"\"")}\""));

        return string.Join(Environment.NewLine, lines);
    }
}

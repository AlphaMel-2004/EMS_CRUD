namespace CrudLearning.Api.DTOs.Reports;

public sealed record AttendanceSummaryResponse(
    DateOnly Date,
    int TotalEmployees,
    int CheckedIn,
    int CheckedOut,
    int Absent,
    int OnLeave);

public sealed record MonthlyAttendanceSummaryResponse(
    int Year,
    int Month,
    int TotalCheckIns,
    int TotalCheckOuts,
    int UniqueEmployeesWithActivity,
    int TotalWorkDays);

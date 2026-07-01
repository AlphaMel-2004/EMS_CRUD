using CrudLearning.Api.Models;

namespace CrudLearning.Api.DTOs.Attendance;

public sealed record AttendanceEntryResponse(
    int Id,
    int EmployeeId,
    string EmployeeName,
    AttendanceEventType EventType,
    DateTime OccurredAt,
    string? Note);

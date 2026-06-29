namespace CrudLearning.Api.Models;

public class AttendanceEntry
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;
    public AttendanceEventType EventType { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? Note { get; set; }
}
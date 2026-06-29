namespace CrudLearning.Api.Models;

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public EmployeeWorkStatus WorkStatus { get; set; } = EmployeeWorkStatus.Working;
    public AttendanceState AttendanceState { get; set; } = AttendanceState.CheckedOut;
    public bool IsDeleted { get; set; }
    public DateTime? LastCheckInAt { get; set; }
    public DateTime? LastCheckOutAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public AppUser? UserAccount { get; set; }
    public ICollection<AttendanceEntry> AttendanceEntries { get; set; } = new List<AttendanceEntry>();
}
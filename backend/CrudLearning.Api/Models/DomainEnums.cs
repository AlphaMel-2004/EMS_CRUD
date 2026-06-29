namespace CrudLearning.Api.Models;

public enum UserRole
{
    Admin,
    Employee
}

public enum EmployeeWorkStatus
{
    Working,
    Absent,
    Leave
}

public enum AttendanceState
{
    CheckedOut,
    CheckedIn
}

public enum AttendanceEventType
{
    CheckIn,
    CheckOut
}
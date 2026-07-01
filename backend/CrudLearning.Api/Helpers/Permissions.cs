using System.Security.Claims;
using CrudLearning.Api.Security;

namespace CrudLearning.Api.Helpers;

public static class Permissions
{
    public static bool IsAdmin(ClaimsPrincipal user) => user.IsInRole(RoleNames.Admin);

    public static int? GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    public static int? GetEmployeeId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("employee_id");
        return int.TryParse(value, out var id) ? id : null;
    }

    public static bool CanAccessEmployee(ClaimsPrincipal user, int employeeId)
    {
        return IsAdmin(user) || GetEmployeeId(user) == employeeId;
    }
}

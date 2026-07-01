namespace CrudLearning.Api.DTOs.Audit;

public sealed record AuditLogResponse(
    int Id,
    string Action,
    int? ActorUserId,
    int? TargetEmployeeId,
    string Description,
    DateTime CreatedAt,
    string? IpAddress,
    string? UserAgent);

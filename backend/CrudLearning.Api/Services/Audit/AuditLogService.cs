using CrudLearning.Api.Data;
using CrudLearning.Api.DTOs.Audit;
using CrudLearning.Api.Helpers;
using CrudLearning.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Services.Audit;

public sealed class AuditLogService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RecordAsync(string action, int? actorUserId, int? targetEmployeeId, string description)
    {
        var request = _httpContextAccessor.HttpContext?.Request;

        _db.AuditLogs.Add(new AuditLog
        {
            Action = action,
            ActorUserId = actorUserId,
            TargetEmployeeId = targetEmployeeId,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = request?.Headers.UserAgent.ToString()
        });

        await _db.SaveChangesAsync();
    }

    public async Task<PaginatedResponse<AuditLogResponse>> GetLogsAsync(int page, int pageSize)
    {
        page = Pagination.NormalizePage(page);
        pageSize = Pagination.NormalizePageSize(pageSize);

        var query = _db.AuditLogs.AsNoTracking().OrderByDescending(log => log.CreatedAt);
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(log => new AuditLogResponse(
                log.Id,
                log.Action,
                log.ActorUserId,
                log.TargetEmployeeId,
                log.Description,
                log.CreatedAt,
                log.IpAddress,
                log.UserAgent))
            .ToListAsync();

        return new PaginatedResponse<AuditLogResponse>(
            items,
            page,
            pageSize,
            totalItems,
            Pagination.CountTotalPages(totalItems, pageSize));
    }
}

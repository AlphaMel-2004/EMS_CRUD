using CrudLearning.Api.DTOs.Audit;
using CrudLearning.Api.Helpers;
using CrudLearning.Api.Security;
using CrudLearning.Api.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudLearning.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Admin)]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly AuditLogService _auditLogService;

    public AuditLogsController(AuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AuditLogResponse>>> GetAuditLogs(int page = 1, int pageSize = 20)
    {
        return Ok(await _auditLogService.GetLogsAsync(page, pageSize));
    }
}

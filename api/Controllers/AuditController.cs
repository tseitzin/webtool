using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;
using System.Security.Claims;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? email,
        [FromQuery] bool? success)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user?.IsAdmin != true)
        {
            return Forbid();
        }

        var query = _context.AuditLogs.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(l => l.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.Timestamp <= endDate.Value);

        if (!string.IsNullOrEmpty(email))
            query = query.Where(l => l.Email.Contains(email));

        if (success.HasValue)
            query = query.Where(l => l.Success == success.Value);

        var logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Take(1000)
            .ToListAsync();

        return Ok(logs);
    }
}
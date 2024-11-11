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
    private const int DefaultPageSize = 10;

    public AuditController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? email,
        [FromQuery] bool? success,
        [FromQuery] string? sortBy = "timestamp",
        [FromQuery] string? sortOrder = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user?.IsAdmin != true)
        {
            return Forbid();
        }

        var query = _context.AuditLogs.AsQueryable();

        // Apply filters
        if (startDate.HasValue)
            query = query.Where(l => l.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.Timestamp <= endDate.Value);

        if (!string.IsNullOrEmpty(email))
            query = query.Where(l => l.Email.Contains(email));

        if (success.HasValue)
            query = query.Where(l => l.Success == success.Value);

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "email" => sortOrder == "desc" ? query.OrderByDescending(l => l.Email) : query.OrderBy(l => l.Email),
            "event" => sortOrder == "desc" ? query.OrderByDescending(l => l.Event) : query.OrderBy(l => l.Event),
            "success" => sortOrder == "desc" ? query.OrderByDescending(l => l.Success) : query.OrderBy(l => l.Success),
            "ipaddress" => sortOrder == "desc" ? query.OrderByDescending(l => l.IpAddress) : query.OrderBy(l => l.IpAddress),
            _ => sortOrder == "desc" ? query.OrderByDescending(l => l.Timestamp) : query.OrderBy(l => l.Timestamp)
        };

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var validPageSize = Math.Min(Math.Max(1, pageSize), 100); // Ensure pageSize is between 1 and 100
        var validPage = Math.Max(1, page);
        var skip = (validPage - 1) * validPageSize;

        var logs = await query
            .Skip(skip)
            .Take(validPageSize)
            .ToListAsync();

        var response = new
        {
            logs,
            pagination = new
            {
                currentPage = validPage,
                pageSize = validPageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)validPageSize)
            }
        };

        return Ok(response);
    }
}
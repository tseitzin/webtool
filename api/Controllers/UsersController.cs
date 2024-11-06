using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;
using api.Services;
using System.Security.Claims;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public UsersController(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                IsAdmin = u.IsAdmin,
                CreatedDate = u.CreatedDate
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentUser = await _context.Users.FindAsync(currentUserId);

        if (currentUser?.IsAdmin != true)
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.IsAdmin)
        {
            return BadRequest(new { message = "Cannot delete admin users" });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> AdminResetPassword(int id, [FromBody] AdminResetPasswordRequest request)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentUser = await _context.Users.FindAsync(currentUserId);

        if (currentUser?.IsAdmin != true)
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        await _authService.AdminResetPasswordAsync(user.Id, request.NewPassword);
        return Ok(new { message = "Password has been reset successfully" });
    }
}
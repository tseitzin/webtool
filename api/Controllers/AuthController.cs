using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Services;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public AuthController(IAuthService authService, IEmailService emailService)
    {
        _authService = authService;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(AuthRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(new AuthResponse
            {
                Token = response.Token,
                Email = response.Email,
                Name = response.Name,
                IsAdmin = response.IsAdmin,
                LastLoginDate = response.LastLoginDate,
                CreatedDate = response.CreatedDate,
                NumberOfLogins = response.NumberOfLogins,
                FailedLogins = response.FailedLogins
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(new AuthResponse
            {
                Token = response.Token,
                Email = response.Email,
                Name = response.Name,
                IsAdmin = response.IsAdmin,
                LastLoginDate = response.LastLoginDate,
                CreatedDate = response.CreatedDate,
                NumberOfLogins = response.NumberOfLogins,
                FailedLogins = response.FailedLogins
            });
        }
        catch (InvalidOperationException)
        {
            return BadRequest(new { message = "Invalid credentials" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        try
        {
            await _authService.ForgotPasswordAsync(request);
            return Ok(new { message = "If your email is registered, you will receive a password reset link." });
        }
        catch (Exception)
        {
            // Return the same message even if email doesn't exist (security best practice)
            return Ok(new { message = "If your email is registered, you will receive a password reset link." });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(new { message = "Password has been reset successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
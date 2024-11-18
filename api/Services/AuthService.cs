using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public AuthService(AppDbContext context, IConfiguration configuration, 
        IEmailService emailService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResponse> RegisterAsync(AuthRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            await LogAuthEvent(request.Email, false, "Email already exists", "Registration Failed");
            throw new InvalidOperationException("User already exists");
        }

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            PasswordHash = HashPassword(request.Password),
            IsAdmin = false,
            CreatedDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await LogAuthEvent(request.Email, true, null, "Registration Success");

        return new AuthResponse
        {
            Token = GenerateJwtToken(user),
            Email = user.Email,
            Name = user.Name,
            IsAdmin = user.IsAdmin
        };
    }

    public async Task<AuthResponse> LoginAsync(AuthRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            await LogAuthEvent(request.Email, false, "User not found", "Login Failed");
            throw new InvalidOperationException("Invalid credentials");
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            await LogAuthEvent(request.Email, false, "Invalid password", "Login Failed");
            user.FailedLogins++;
            await _context.SaveChangesAsync();
            throw new InvalidOperationException("Invalid credentials");
        }

        user.LastLoginDate = DateTime.UtcNow;
        user.NumberOfLogins++;
        await _context.SaveChangesAsync();

        await LogAuthEvent(request.Email, true, null, "Login Success");

        return new AuthResponse
        {
            Token = GenerateJwtToken(user),
            Email = user.Email,
            Name = user.Name,
            IsAdmin = user.IsAdmin
        };
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return;

        var tokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");

        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync();

        var resetLink = $"{_configuration["ClientUrl"]}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
        var emailBody = $@"
            <h2>Reset Your Password</h2>
            <p>Click the link below to reset your password:</p>
            <a href='{resetLink}'>Reset Password</a>
            <p>This link will expire in 1 hour.</p>";

        await LogAuthEvent(request.Email, true, "N/A", "Password reset sent");

        await _emailService.SendEmailAsync(
            user.Email,
            "Password Reset Request",
            emailBody);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.Email == request.Email && 
            u.ResetToken == request.Token &&
            u.ResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            await LogAuthEvent("null", false, "Password reset token failure", "Password reset by User");
            throw new InvalidOperationException("Invalid or expired reset token");
        }

        user.PasswordHash = HashPassword(request.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await LogAuthEvent(user.Email, true, "N/A", "Password reset by User");

        await _context.SaveChangesAsync();
    }

    public async Task AdminResetPasswordAsync(int userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.PasswordHash = HashPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await LogAuthEvent(user.Email, true, "N/A", "Password reset by Admin");

        await _context.SaveChangesAsync();
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("IsAdmin", user.IsAdmin.ToString().ToLower())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task LogAuthEvent(string email, bool success, 
                                    string? failureReason, string curEvent)
    {
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        
        var log = new AuditLog
        {
            Event = curEvent,
            Email = email,
            Success = success,
            FailureReason = failureReason,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
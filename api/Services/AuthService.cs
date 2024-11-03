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

    public AuthService(AppDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<AuthResponse> RegisterAsync(AuthRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new InvalidOperationException("User already exists");
        }

        var user = new User
        {
            Email = request.Email,
            Name = request.Email.Split('@')[0],
            PasswordHash = HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = GenerateJwtToken(user),
            Email = user.Email,
            Name = user.Name
        };
    }

    public async Task<AuthResponse> LoginAsync(AuthRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid credentials");
        }

        return new AuthResponse
        {
            Token = GenerateJwtToken(user),
            Email = user.Email,
            Name = user.Name
        };
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return; // Don't reveal if email exists

        // Generate reset token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync();

        var resetLink = $"{_configuration["ClientUrl"]}/reset-password?token={token}&email={user.Email}";
        var emailBody = $@"
            <h2>Reset Your Password</h2>
            <p>Click the link below to reset your password:</p>
            <a href='{resetLink}'>Reset Password</a>
            <p>This link will expire in 1 hour.</p>";

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
            throw new InvalidOperationException("Invalid or expired reset token");
        }

        user.PasswordHash = HashPassword(request.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

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
            new Claim(ClaimTypes.Name, user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
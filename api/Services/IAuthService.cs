using api.Models;

namespace api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(AuthRequest request);
    Task<AuthResponse> LoginAsync(AuthRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}
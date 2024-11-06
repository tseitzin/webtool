namespace api.Models;

public class AuthResponse
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required bool IsAdmin { get; set; }
}
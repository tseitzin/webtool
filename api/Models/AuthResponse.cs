namespace api.Models;

public class AuthResponse
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required bool IsAdmin { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int NumberOfLogins { get; set; }
    public int FailedLogins { get; set; }
}
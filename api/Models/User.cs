namespace api.Models;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public bool IsAdmin { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? PreviousLoginDate { get; set; }
    public int NumberOfLogins { get; set; }
    public int FailedLogins { get; set; }

}
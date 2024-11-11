namespace api.Models;

public class UserDto
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public bool IsAdmin { get; set; }
    public string? CreatedDate { get; set; }
    public string? LastLoginDate { get; set; }
    public int NumberOfLogins { get; set; }
    public int FailedLogins { get; set; }
}
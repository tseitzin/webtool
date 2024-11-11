namespace api.Models;

public class AuditLog
{
    public int Id { get; set; }
    public required string Event { get; set; }
    public required string Email { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public required DateTime Timestamp { get; set; }
}
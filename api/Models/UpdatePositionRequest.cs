namespace api.Models;

public class UpdatePositionRequest
{
    public required decimal Quantity { get; set; }
    public string? Notes { get; set; }
}
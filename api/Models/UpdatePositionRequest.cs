namespace api.Models;

public class UpdatePositionRequest
{
    public required decimal QuantityToBuy { get; set; }
    public required decimal QuantityAlreadyOwned { get; set; }
    public string? Notes { get; set; }
}
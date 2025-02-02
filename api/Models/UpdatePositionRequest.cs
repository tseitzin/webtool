namespace api.Models;

public class UpdatePositionRequest
{
    public required decimal QuantityToBuy { get; set; }
    public required decimal QuantityAlreadyOwned { get; set; }
    public required decimal PurchasePrice { get; set; }
    public required DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
}
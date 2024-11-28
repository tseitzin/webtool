namespace api.Models;

public class AddPositionRequest
{
    public required string Symbol { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal PurchasePrice { get; set; }
    public required DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
}
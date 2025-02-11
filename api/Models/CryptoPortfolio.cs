namespace api.Models;

public class CryptoPortfolio
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required string Symbol { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal PurchasePrice { get; set; }
    public required decimal AveragePurchasePrice { get; set; }
    public required decimal TotalCost { get; set; }
    public required decimal CurrentValue { get; set; }
    public required DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
    public User User { get; set; } = null!;
}
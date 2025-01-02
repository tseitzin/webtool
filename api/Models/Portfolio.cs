namespace api.Models;

public class Portfolio
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public required string Symbol { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal PurchasePrice { get; set; }
    public required DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
    public User User { get; set; } = null!;
}

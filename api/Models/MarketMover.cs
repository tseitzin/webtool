namespace api.Models;

public class MarketMover
{
    public int Id { get; set; }
    public required string Symbol { get; set; }
    public decimal Price { get; set; }
    public decimal ChangePercent { get; set; }
    public long Volume { get; set; }
    public string Type { get; set; } = "gainer"; // "gainer" or "loser"
    public DateTime LastUpdated { get; set; }
}
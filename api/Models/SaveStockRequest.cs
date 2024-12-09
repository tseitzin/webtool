namespace api.Models;

public class SaveStockRequest
{
    public required string CompanyName { get; set; }
    public required decimal Price { get; set; }
    public required decimal Change { get; set; }
    public required decimal ChangePercent { get; set; }
    public required long Volume { get; set; }
    public required decimal MarketCap { get; set; }
    public decimal? Open { get; set; }
    public decimal? High { get; set; }
    public decimal? Low { get; set; }
    public decimal? PreviousClose { get; set; }
}
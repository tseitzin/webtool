using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class StockData
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(10)]
    public required string Symbol { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public decimal Change { get; set; }
    
    public decimal ChangePercent { get; set; }
    
    [Required]
    public long Volume { get; set; }
    
    public decimal MarketCap { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    public decimal? Open { get; set; }
    
    public decimal? High { get; set; }
    
    public decimal? Low { get; set; }
    
    public decimal? PreviousClose { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class UserSavedStock
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public required string Symbol { get; set; }

    [Required]
    [MaxLength(100)]
    public required string CompanyName { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public decimal Change { get; set; }
    
    public decimal ChangePercent { get; set; }
    
    [Required]
    public long Volume { get; set; }
    
    public decimal MarketCap { get; set; }
    
    public DateTime SavedAt { get; set; }
    
    public decimal? Open { get; set; }
    
    public decimal? High { get; set; }
    
    public decimal? Low { get; set; }
    
    public decimal? PreviousClose { get; set; }
    
    public User User { get; set; } = null!;
}
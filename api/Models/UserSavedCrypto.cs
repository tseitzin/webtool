using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class UserSavedCrypto
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public required string Symbol { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    public decimal Open { get; set; }
    
    public decimal Change { get; set; }
    
    public decimal ChangePercent { get; set; }
    
    [Required]
    public decimal Volume { get; set; }
    
    public decimal High24h { get; set; }
    
    public decimal Low24h { get; set; }
    
    public DateTime SavedAt { get; set; }
    
    public User User { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class UserOwnedStock
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public required string Symbol { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    public decimal PurchasePrice { get; set; }
    
    public DateTime PurchaseDate { get; set; }
    
    public string? Notes { get; set; }
    
    public User User { get; set; } = null!;
}
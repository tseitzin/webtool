using System.ComponentModel.DataAnnotations;

namespace api.Models;

public class UserFavoriteStock
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public required string Symbol { get; set; }
    
    public DateTime AddedAt { get; set; }
    
    public User User { get; set; } = null!;
}
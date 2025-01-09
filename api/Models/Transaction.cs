using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public required int UserId { get; set; }

        [Required]
        [MaxLength(10)]
        public required string StockSymbol { get; set; }

        [Required]
        public required string TransactionType { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        public DateTime TransactionDate { get; set; }
        public User User { get; set; } = null!;
    }
}
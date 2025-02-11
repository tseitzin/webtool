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
        public required string Symbol { get; set; }

        [Required]
        public required string Type { get; set; } // "STOCK" or "CRYPTO"

        [Required]
        public required string TransactionType { get; set; } // "BUY" or "SELL"

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public decimal TransactionTotal { get; set; }

        public DateTime TransactionDate { get; set; }
        public User User { get; set; } = null!;
    }
}

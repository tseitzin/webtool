// api/Controllers/PortfolioController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;
using System.Security.Claims;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly AppDbContext _context;

    public PortfolioController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Portfolio>>> GetPortfolio()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var portfolio = await _context.UserOwnedStocks
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Symbol)
            .ToListAsync();

        return Ok(portfolio);
    }

    // [HttpPost]
    // public async Task<ActionResult<Portfolio>> AddToPortfolio([FromBody] AddToPortfolioRequest request)
    // {
    //     var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
    //     var portfolio = new Portfolio
    //     {
    //         UserId = userId,
    //         Symbol = request.Symbol.ToUpper(),
    //         Quantity = request.Quantity,
    //         PurchasePrice = request.PurchasePrice,
    //         PurchaseDate = DateTime.UtcNow,
    //         Notes = request.Notes
    //     };

    //     _context.Portfolios.Add(portfolio);
    //     await _context.SaveChangesAsync();

    //     return Ok(portfolio);
    // }

    [HttpPost("{id}/sell")]
    public async Task<ActionResult<UserOwnedStock>> SellPosition(int id, [FromBody] SellPositionRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var position = await _context.UserOwnedStocks
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (position == null)
        {
            return NotFound();
        }

        if (request.Quantity > position.Quantity)
        {
            return BadRequest("Cannot sell more shares than owned");
        }

        try
        {
            position.Quantity -= request.Quantity;

            // Add transaction record
            var transaction = new Transaction
            {
                UserId = userId,
                StockSymbol = position.Symbol,
                TransactionType = "SELL",
                Quantity = (int)request.Quantity,
                Price = position.PurchasePrice, // You might want to update this to current market price
                TransactionDate = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            if (position.Quantity == 0)
            {
                _context.UserOwnedStocks.Remove(position);
            }

            position.CurrentValue -= (request.Quantity * position.PurchasePrice);
            position.TotalCost -= (request.Quantity * position.PurchasePrice); 

            await _context.SaveChangesAsync();
            return Ok(position);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePosition(int id, [FromBody] UpdatePositionRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var position = await _context.UserOwnedStocks
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (position == null)
        {
            return NotFound();
        }

        // Validate quantity
        if (request.Quantity < 0)
        {
            return BadRequest("Quantity cannot be negative");
        }

        try 
        {
            decimal quantityDifference = request.Quantity - position.Quantity;
            
            if (quantityDifference != 0)
            {
                // Add transaction record
                var transaction = new Transaction
                {
                    UserId = userId,
                    StockSymbol = position.Symbol,
                    TransactionType = quantityDifference > 0 ? "BUY" : "SELL",
                    Quantity = Math.Abs((int)quantityDifference),
                    Price = position.PurchasePrice,
                    TransactionDate = DateTime.UtcNow
                };
                _context.Transactions.Add(transaction);
            }

            // Update position
            position.Quantity = request.Quantity;
            position.CurrentValue += position.TotalCost;
            
            // Update notes if provided
            if (request.Notes != null)
            {
                position.Notes = request.Notes;
            }

            await _context.SaveChangesAsync();
            return Ok(position);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPost]
    public async Task<ActionResult<UserOwnedStock>> AddPosition([FromBody] AddPositionRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Check if user already owns this stock
        var existingPosition = await _context.UserOwnedStocks
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Symbol == request.Symbol.ToUpper());

        try
        {
            if (existingPosition != null)
            {
                // Update existing position
                var totalCost = (existingPosition.Quantity * existingPosition.PurchasePrice) + 
                       (request.Quantity * request.PurchasePrice);
                var newTotalQuantity = existingPosition.Quantity + request.Quantity;
                var newAveragePurchasePrice = totalCost / newTotalQuantity;

                existingPosition.Quantity = newTotalQuantity;
                existingPosition.PurchasePrice = newAveragePurchasePrice;
                existingPosition.Notes = request.Notes ?? existingPosition.Notes;
            }
            else
            {
                // Create new position
                var position = new UserOwnedStock
                {
                    UserId = userId,
                    Symbol = request.Symbol.ToUpper(),
                    Quantity = request.Quantity,
                    PurchasePrice = request.PurchasePrice,
                    AveragePurchasePrice = request.PurchasePrice,
                    TotalCost = request.Quantity * request.PurchasePrice,
                    PurchaseDate = request.PurchaseDate.ToUniversalTime(),
                    Notes = request.Notes
                };
                position.CurrentValue += position.TotalCost;
                _context.UserOwnedStocks.Add(position);
            }

            // Add transaction record
            var transaction = new Transaction
            {
                UserId = userId,
                StockSymbol = request.Symbol.ToUpper(),
                TransactionType = "BUY",
                Quantity = (int)request.Quantity,
                Price = request.PurchasePrice,
                TransactionDate = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        return Ok(transactions);
    }


    [HttpPost("/portfolio")]
    public async Task<ActionResult<Transaction>> AddTransaction([FromBody] Transaction request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var savedTransaction = new Transaction
        {
            UserId = userId,
            StockSymbol = request.StockSymbol,
            TransactionType = request.TransactionType,
            TransactionDate = DateTime.Now,
            Quantity = request.Quantity,
            Price = request.Price,
        };

        _context.Add(savedTransaction);

        await _context.SaveChangesAsync();
        return Ok();
    }


}

public class AddToPortfolioRequest
{
    public required string Symbol { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal PurchasePrice { get; set; }
    public string? Notes { get; set; }
}

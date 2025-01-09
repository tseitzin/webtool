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

        position.Quantity -= request.Quantity;

        if (position.Quantity == 0)
        {
            _context.UserOwnedStocks.Remove(position);
        }

        await _context.SaveChangesAsync();
        return Ok(position);
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

        // Update position
        position.Quantity = request.Quantity;
        
        // Update notes if provided
        if (request.Notes != null)
        {
            position.Notes = request.Notes;
        }

        await _context.SaveChangesAsync();
        return Ok(position);
    }

    [HttpPost]
    public async Task<ActionResult<UserOwnedStock>> AddPosition([FromBody] AddPositionRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Check if user already owns this stock
        var existingPosition = await _context.UserOwnedStocks
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Symbol == request.Symbol.ToUpper());

        Console.WriteLine("Existing position: " + existingPosition?.UserId);
        if (existingPosition != null)
        {
            Console.WriteLine("Existing position quantity =" + existingPosition.Quantity);
            // Update existing position
             existingPosition.Quantity += request.Quantity;
            // Optionally update average purchase price
            existingPosition.PurchasePrice = 
                ((existingPosition.Quantity - request.Quantity) * existingPosition.PurchasePrice + 
                request.Quantity * request.PurchasePrice) / existingPosition.Quantity;
            existingPosition.Notes = request.Notes ?? existingPosition.Notes;
            
            await _context.SaveChangesAsync();
            return Ok(existingPosition);
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
                PurchaseDate = request.PurchaseDate.ToUniversalTime(),
                Notes = request.Notes
            };
            _context.UserOwnedStocks.Add(position);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions([FromBody] AddPositionRequest request)
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

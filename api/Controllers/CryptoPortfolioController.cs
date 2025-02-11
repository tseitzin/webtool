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
public class CryptoPortfolioController : ControllerBase
{
    private readonly AppDbContext _context;

    public CryptoPortfolioController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CryptoPortfolio>>> GetPortfolio()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var portfolio = await _context.CryptoPortfolios
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Symbol)
            .ToListAsync();

        return Ok(portfolio);
    }

    [HttpPost]
    public async Task<ActionResult<CryptoPortfolio>> AddPosition([FromBody] AddPositionRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        try
        {
            decimal transactionAmount = request.Quantity * request.PurchasePrice;

            var position = new CryptoPortfolio
            {
                UserId = userId,
                Symbol = request.Symbol.ToUpper(),
                Quantity = request.Quantity,
                PurchasePrice = request.PurchasePrice,
                AveragePurchasePrice = request.PurchasePrice,
                TotalCost = transactionAmount,
                CurrentValue = transactionAmount,
                PurchaseDate = request.PurchaseDate.ToUniversalTime(),
                Notes = request.Notes
            };

            _context.CryptoPortfolios.Add(position);

            // Add transaction record
            var transaction = new Transaction
            {
                UserId = userId,
                StockSymbol = request.Symbol.ToUpper(),
                TransactionType = "BUY",
                Quantity = (int)request.Quantity,
                Price = request.PurchasePrice,
                TransactionTotal = transactionAmount,
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePosition(int id, [FromBody] UpdatePositionRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var position = await _context.CryptoPortfolios
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (position == null)
        {
            return NotFound();
        }

        try 
        {
            decimal transactionAmount = request.QuantityToBuy * request.PurchasePrice;
            
            // Add transaction record
            var transaction = new Transaction
            {
                UserId = userId,
                StockSymbol = position.Symbol,
                TransactionType = request.QuantityToBuy > 0 ? "BUY" : "SELL",
                Quantity = (int)request.QuantityToBuy,
                Price = request.PurchasePrice,
                TransactionTotal = transactionAmount,
                TransactionDate = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);
    
            // Update position
            position.Quantity = request.QuantityAlreadyOwned;
            position.CurrentValue = position.CurrentValue + transactionAmount;
            position.TotalCost = position.TotalCost + transactionAmount;
            
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

    [HttpPost("{id}/sell")]
    public async Task<ActionResult<CryptoPortfolio>> SellPosition(int id, [FromBody] SellPositionRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var position = await _context.CryptoPortfolios
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (position == null)
        {
            return NotFound();
        }

        if (request.Quantity > position.Quantity)
        {
            return BadRequest("Cannot sell more than owned");
        }

        try
        {
            position.Quantity -= request.Quantity;
            decimal transactionAmount = request.Quantity * (position.PurchasePrice * -1);

            // Add transaction record
            var transaction = new Transaction
            {
                UserId = userId,
                StockSymbol = position.Symbol,
                TransactionType = "SELL",
                Quantity = (int)request.Quantity,
                Price = position.PurchasePrice,
                TransactionTotal = transactionAmount,
                TransactionDate = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            if (position.Quantity == 0)
            {
                _context.CryptoPortfolios.Remove(position);
            }
            else
            {
                position.CurrentValue = position.Quantity * position.PurchasePrice;
                position.TotalCost = position.Quantity * position.AveragePurchasePrice;
            }

            await _context.SaveChangesAsync();
            return Ok(position);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
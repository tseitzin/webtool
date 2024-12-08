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
public class SavedStocksController : ControllerBase
{
    private readonly AppDbContext _context;

    public SavedStocksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSavedStock>>> GetSavedStocks()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var savedStocks = await _context.UserSavedStocks
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SavedAt)
            .ToListAsync();

        return Ok(savedStocks);
    }

    [HttpPost("{symbol}")]
    public async Task<ActionResult<UserSavedStock>> SaveStock(string symbol, [FromBody] SaveStockRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Check if already saved
        var existing = await _context.UserSavedStocks
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Symbol == symbol.ToUpper());

        if (existing != null)
        {
            // Update existing saved stock
            existing.Price = request.Price;
            existing.Change = request.Change;
            existing.ChangePercent = request.ChangePercent;
            existing.Volume = request.Volume;
            existing.MarketCap = request.MarketCap;
            existing.SavedAt = DateTime.UtcNow;
            existing.Open = request.Open;
            existing.High = request.High;
            existing.Low = request.Low;
            existing.PreviousClose = request.PreviousClose;
        }
        else
        {
            // Create new saved stock
            var savedStock = new UserSavedStock
            {
                UserId = userId,
                Symbol = symbol.ToUpper(),
                Price = request.Price,
                Change = request.Change,
                ChangePercent = request.ChangePercent,
                Volume = request.Volume,
                MarketCap = request.MarketCap,
                SavedAt = DateTime.UtcNow,
                Open = request.Open,
                High = request.High,
                Low = request.Low,
                PreviousClose = request.PreviousClose
            };

            _context.UserSavedStocks.Add(savedStock);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{symbol}")]
    public async Task<IActionResult> RemoveSavedStock(string symbol)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var savedStock = await _context.UserSavedStocks
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Symbol == symbol.ToUpper());

        if (savedStock == null)
        {
            return NotFound();
        }

        _context.UserSavedStocks.Remove(savedStock);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
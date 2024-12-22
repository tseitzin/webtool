using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketMoversController : ControllerBase
{
    private readonly AppDbContext _context;

    public MarketMoversController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMarketMovers()
    {
        var gainers = await _context.MarketMovers
            .Where(m => m.Type == "gainer")
            .OrderByDescending(m => m.ChangePercent)
            .Take(10)
            .ToListAsync();

        var losers = await _context.MarketMovers
            .Where(m => m.Type == "loser")
            .OrderBy(m => m.ChangePercent)
            .Take(10)
            .ToListAsync();

        var lastUpdate = await _context.MarketMovers
            .OrderByDescending(m => m.LastUpdated)
            .Select(m => m.LastUpdated)
            .FirstOrDefaultAsync();

        return Ok(new { 
            gainers, 
            losers,
            lastUpdate,
            marketStatus = DateTime.UtcNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday 
                ? "Market is closed for the weekend. Showing data from last trading day."
                : null
        });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMarketMovers([FromBody] MarketMoversUpdateRequest request)
    {
        // Only update if we have new data
        if (request.Gainers.Any() || request.Losers.Any())
        {
            // Remove old data
            await _context.MarketMovers.ExecuteDeleteAsync();

            var now = DateTime.UtcNow;
            var movers = new List<MarketMover>();

            // Add gainers
            movers.AddRange(request.Gainers.Select(g => new MarketMover
            {
                Symbol = g.Symbol,
                Price = g.Price,
                ChangePercent = g.ChangePercent,
                Volume = g.Volume,
                Type = "gainer",
                LastUpdated = now
            }));

            // Add losers
            movers.AddRange(request.Losers.Select(l => new MarketMover
            {
                Symbol = l.Symbol,
                Price = l.Price,
                ChangePercent = l.ChangePercent,
                Volume = l.Volume,
                Type = "loser",
                LastUpdated = now
            }));

            await _context.MarketMovers.AddRangeAsync(movers);
            await _context.SaveChangesAsync();
        }

        return Ok();
    }
}

public class MarketMoversUpdateRequest
{
    public required List<MarketMoverData> Gainers { get; set; }
    public required List<MarketMoverData> Losers { get; set; }
}

public class MarketMoverData
{
    public required string Symbol { get; set; }
    public decimal Price { get; set; }
    public decimal ChangePercent { get; set; }
    public long Volume { get; set; }
}
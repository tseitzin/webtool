using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StockDataController : ControllerBase
{
    private readonly AppDbContext _context;

    public StockDataController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockData>>> GetStockData(
        [FromQuery] string? symbol,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var query = _context.StockData.AsQueryable();

        if (!string.IsNullOrEmpty(symbol))
        {
            query = query.Where(s => s.Symbol == symbol.ToUpper());
        }

        if (startDate.HasValue)
        {
            query = query.Where(s => s.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.Timestamp <= endDate.Value);
        }

        var stocks = await query
            .OrderByDescending(s => s.Timestamp)
            .Take(100) // Limit results
            .ToListAsync();

        return Ok(stocks);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetMarketSummary()
    {
        var latestData = await _context.StockData
            .GroupBy(s => s.Symbol)
            .Select(g => g.OrderByDescending(s => s.Timestamp).First())
            .ToListAsync();

        var marketSummary = new
        {
            TotalVolume = latestData.Sum(s => s.Volume),
            AverageChange = latestData.Average(s => s.ChangePercent),
            TopGainers = latestData.OrderByDescending(s => s.ChangePercent).Take(5),
            TopLosers = latestData.OrderBy(s => s.ChangePercent).Take(5),
            MostActive = latestData.OrderByDescending(s => s.Volume).Take(5)
        };

        return Ok(marketSummary);
    }

    [HttpGet("{symbol}")]
    public async Task<ActionResult<StockData>> GetStockBySymbol(string symbol)
    {
        var stock = await _context.StockData
            .Where(s => s.Symbol == symbol.ToUpper())
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync();

        if (stock == null)
        {
            return NotFound();
        }

        return Ok(stock);
    }
}
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
    public async Task<ActionResult<IEnumerable<UserOwnedStock>>> GetPortfolio()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var portfolio = await _context.UserOwnedStocks
            .Where(o => o.UserId == userId)
            .OrderBy(o => o.Symbol)
            .ToListAsync();

        return Ok(portfolio);
    }

    [HttpPost]
    public async Task<ActionResult<UserOwnedStock>> AddPosition([FromBody] AddPositionRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // Check if stock exists
        var stockExists = await _context.StockData
            .AnyAsync(s => s.Symbol == request.Symbol.ToUpper());

        if (!stockExists)
        {
            return NotFound("Stock not found");
        }

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

        position.Quantity = request.Quantity;
        position.Notes = request.Notes;

        await _context.SaveChangesAsync();
        return Ok(position);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemovePosition(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var position = await _context.UserOwnedStocks
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (position == null)
        {
            return NotFound();
        }

        _context.UserOwnedStocks.Remove(position);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
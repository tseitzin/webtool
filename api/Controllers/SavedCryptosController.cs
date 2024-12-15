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
public class SavedCryptosController : ControllerBase
{
    private readonly AppDbContext _context;

    public SavedCryptosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSavedCrypto>>> GetSavedCryptos()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var savedCryptos = await _context.UserSavedCryptos
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.SavedAt)
            .ToListAsync();

        return Ok(savedCryptos);
    }

    [HttpPost("{symbol}")]
    public async Task<ActionResult<UserSavedCrypto>> SaveCrypto(string symbol, [FromBody] SaveCryptoRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Check if already saved
        var existing = await _context.UserSavedCryptos
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Symbol == symbol.ToUpper());

        if (existing != null)
        {
            // Update existing saved crypto
            existing.Price = request.Price;
            existing.Open = request.Open;
            existing.Change = request.Change;
            existing.ChangePercent = request.ChangePercent;
            existing.Volume = request.Volume;
            existing.SavedAt = DateTime.UtcNow;
            existing.High24h = request.High24h;
            existing.Low24h = request.Low24h;
        }
        else
        {
            // Create new saved crypto
            var savedCrypto = new UserSavedCrypto
            {
                UserId = userId,
                Symbol = symbol.ToUpper(),
                Price = request.Price,
                Open = request.Open,
                Change = request.Change,
                ChangePercent = request.ChangePercent,
                Volume = request.Volume,
                SavedAt = DateTime.UtcNow,
                High24h = request.High24h,
                Low24h = request.Low24h
            };

            _context.UserSavedCryptos.Add(savedCrypto);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{symbol}")]
    public async Task<IActionResult> RemoveSavedCrypto(string symbol)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var savedCrypto = await _context.UserSavedCryptos
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Symbol == symbol.ToUpper());

        if (savedCrypto == null)
        {
            return NotFound();
        }

        _context.UserSavedCryptos.Remove(savedCrypto);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
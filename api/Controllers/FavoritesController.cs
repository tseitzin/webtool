// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using api.Data;
// using api.Models;
// using System.Security.Claims;

// namespace api.Controllers;

// [Authorize]
// [ApiController]
// [Route("api/[controller]")]
// public class FavoritesController : ControllerBase
// {
//     private readonly AppDbContext _context;

//     public FavoritesController(AppDbContext context)
//     {
//         _context = context;
//     }

//     [HttpGet]
//     public async Task<ActionResult<IEnumerable<UserFavoriteStock>>> GetFavorites()
//     {
//         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
//         var favorites = await _context.UserFavoriteStocks
//             .Where(f => f.UserId == userId)
//             .OrderBy(f => f.Symbol)
//             .ToListAsync();

//         return Ok(favorites);
//     }

//     [HttpPost("{symbol}")]
//     public async Task<ActionResult<UserFavoriteStock>> AddFavorite(string symbol)
//     {
//         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
//         // Check if stock exists
//         var stockExists = await _context.StockData
//             .AnyAsync(s => s.Symbol == symbol.ToUpper());

//         if (!stockExists)
//         {
//             return NotFound("Stock not found");
//         }

//         // Check if already favorited
//         var existing = await _context.UserFavoriteStocks
//             .FirstOrDefaultAsync(f => f.UserId == userId && f.Symbol == symbol.ToUpper());

//         if (existing != null)
//         {
//             return BadRequest("Stock already in favorites");
//         }

//         var favorite = new UserFavoriteStock
//         {
//             UserId = userId,
//             Symbol = symbol.ToUpper(),
//             AddedDate = DateTime.UtcNow
//         };

//         _context.UserFavoriteStocks.Add(favorite);
//         await _context.SaveChangesAsync();

//         return Ok(favorite);
//     }

//     [HttpDelete("{symbol}")]
//     public async Task<IActionResult> RemoveFavorite(string symbol)
//     {
//         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
//         var favorite = await _context.UserFavoriteStocks
//             .FirstOrDefaultAsync(f => f.UserId == userId && f.Symbol == symbol.ToUpper());

//         if (favorite == null)
//         {
//             return NotFound();
//         }

//         _context.UserFavoriteStocks.Remove(favorite);
//         await _context.SaveChangesAsync();

//         return NoContent();
//     }
// }
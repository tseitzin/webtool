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
public class TransactionController : ControllerBase
{
    private readonly AppDbContext _context;

    public TransactionController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions(
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    [FromQuery] string? symbol,
    [FromQuery] string? type,
    [FromQuery] string? sortBy = "transactionDate",
    [FromQuery] string? sortOrder = "desc")
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    var query = _context.Transactions.Where(t => t.UserId == userId);

    // Apply filters
    if (startDate.HasValue)
        query = query.Where(t => t.TransactionDate >= startDate.Value);

    if (endDate.HasValue)
        query = query.Where(t => t.TransactionDate <= endDate.Value);

    if (!string.IsNullOrEmpty(symbol))
        query = query.Where(t => t.StockSymbol.ToUpper().Contains(symbol.ToUpper()));

    if (!string.IsNullOrEmpty(type))
        query = query.Where(t => t.TransactionType == type);

    // Apply sorting
    query = sortBy?.ToLower() switch
    {
        "stocksymbol" => sortOrder == "desc" ? query.OrderByDescending(t => t.StockSymbol) : query.OrderBy(t => t.StockSymbol),
        "transactiontype" => sortOrder == "desc" ? query.OrderByDescending(t => t.TransactionType) : query.OrderBy(t => t.TransactionType),
        "quantity" => sortOrder == "desc" ? query.OrderByDescending(t => t.Quantity) : query.OrderBy(t => t.Quantity),
        "price" => sortOrder == "desc" ? query.OrderByDescending(t => t.Price) : query.OrderBy(t => t.Price),
        "transactiontotal" => sortOrder == "desc" ? query.OrderByDescending(t => t.TransactionTotal) : query.OrderBy(t => t.TransactionTotal),
        _ => sortOrder == "desc" ? query.OrderByDescending(t => t.TransactionDate) : query.OrderBy(t => t.TransactionDate)
    };

    var transactions = await query.ToListAsync();
    return Ok(transactions);
}


    [HttpPost]
    public async Task<ActionResult<Transaction>> AddTransaction([FromBody] Transaction transaction)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var newTransaction = new Transaction
        {
            UserId = userId,
            StockSymbol = transaction.StockSymbol,
            TransactionType = transaction.TransactionType,
            Quantity = transaction.Quantity,
            Price = transaction.Price,
            TransactionTotal = transaction.TransactionTotal,
            TransactionDate = DateTime.UtcNow
        };

        _context.Transactions.Add(newTransaction);
        await _context.SaveChangesAsync();

        return Ok(newTransaction);
    }

    [HttpGet("symbol/{symbol}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsBySymbol(string symbol)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.StockSymbol == symbol.ToUpper())
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetTransactionSummary()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var summary = new
        {
            TotalBuys = transactions.Count(t => t.TransactionType == "BUY"),
            TotalSells = transactions.Count(t => t.TransactionType == "SELL"),
            TotalBuyAmount = transactions.Where(t => t.TransactionType == "BUY")
                                      .Sum(t => t.TransactionTotal),
            TotalSellAmount = transactions.Where(t => t.TransactionType == "SELL")
                                       .Sum(t => t.TransactionTotal)
        };

        return Ok(summary);
    }
}
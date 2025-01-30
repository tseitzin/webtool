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
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

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
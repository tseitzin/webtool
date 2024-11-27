using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class StockSeederService
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();
    private readonly string[] _symbols = { "AAPL", "MSFT", "GOOGL", "AMZN", "META", "TSLA", "NVDA", "JPM", "BAC", "WMT" };

    public StockSeederService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedInitialDataAsync()
    {
        if (await _context.StockData.AnyAsync())
        {
            return; // Data already exists
        }

        var baseDate = DateTime.UtcNow.Date.AddDays(-30); // Start from 30 days ago
        var stocks = new List<StockData>();

        foreach (var symbol in _symbols)
        {
            var basePrice = GetBasePrice(symbol);
            var previousClose = basePrice;

            for (var day = 0; day < 30; day++)
            {
                var date = baseDate.AddDays(day);
                var dailyFluctuation = (decimal)(_random.NextDouble() * 0.06 - 0.03); // ±3% daily change
                var price = basePrice * (1 + dailyFluctuation);
                var change = price - previousClose;
                var changePercent = (change / previousClose) * 100;

                var stock = new StockData
                {
                    Symbol = symbol,
                    Price = Math.Round(price, 2),
                    Change = Math.Round(change, 2),
                    ChangePercent = Math.Round(changePercent, 2),
                    Volume = _random.NextInt64(1000000, 10000000),
                    MarketCap = Math.Round(price * GetSharesOutstanding(symbol), 2),
                    Timestamp = date,
                    Open = Math.Round(previousClose * (1 + (decimal)(_random.NextDouble() * 0.02 - 0.01)), 2),
                    High = Math.Round(price * (1 + (decimal)(_random.NextDouble() * 0.01)), 2),
                    Low = Math.Round(price * (1 - (decimal)(_random.NextDouble() * 0.01)), 2),
                    PreviousClose = Math.Round(previousClose, 2)
                };

                stocks.Add(stock);
                previousClose = price;
            }
        }

        await _context.StockData.AddRangeAsync(stocks);
        await _context.SaveChangesAsync();
    }

    private decimal GetBasePrice(string symbol) => symbol switch
    {
        "AAPL" => 175.0m,
        "MSFT" => 350.0m,
        "GOOGL" => 140.0m,
        "AMZN" => 145.0m,
        "META" => 330.0m,
        "TSLA" => 240.0m,
        "NVDA" => 450.0m,
        "JPM" => 150.0m,
        "BAC" => 32.0m,
        "WMT" => 160.0m,
        _ => 100.0m
    };

    private long GetSharesOutstanding(string symbol) => symbol switch
    {
        "AAPL" => 15500000000,
        "MSFT" => 7420000000,
        "GOOGL" => 12800000000,
        "AMZN" => 10350000000,
        "META" => 2570000000,
        "TSLA" => 3170000000,
        "NVDA" => 2470000000,
        "JPM" => 2900000000,
        "BAC" => 7950000000,
        "WMT" => 2690000000,
        _ => 1000000000
    };
}
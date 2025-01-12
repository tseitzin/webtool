// api/Controllers/LogsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly ILogger<LogsController> _logger;
    private readonly string _logPath;

    public LogsController(ILogger<LogsController> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _logPath = Path.Combine(env.ContentRootPath, "logs");
        
        if (!Directory.Exists(_logPath))
        {
            Directory.CreateDirectory(_logPath);
        }
    }

    [HttpPost]
    public async Task<IActionResult> LogEntry([FromBody] LogEntry entry)
    {
        var logFileName = $"app-{DateTime.UtcNow:yyyy-MM-dd}.log";
        var logFilePath = Path.Combine(_logPath, logFileName);

        var logLine = JsonSerializer.Serialize(new {
            timestamp = entry.Timestamp,
            level = entry.Level.ToUpper(),
            message = entry.Message,
            context = entry.Context,
            error = entry.Error,
            user = User.Identity?.Name
        });

        try
        {
            await System.IO.File.AppendAllTextAsync(
                logFilePath, 
                $"{logLine}{Environment.NewLine}"
            );
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write to log file");
            return StatusCode(500, "Failed to write log entry");
        }
    }
}

public class LogEntry
{
    public string Level { get; set; } = "info";
    public string Message { get; set; } = "";
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");
    public object? Context { get; set; }
    public object? Error { get; set; }
}

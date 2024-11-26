using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Services;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SecretsController : ControllerBase
{
    private readonly IKeyVaultService _keyVaultService;

    public SecretsController(IKeyVaultService keyVaultService)
    {
        _keyVaultService = keyVaultService;
    }

    [HttpGet("test")]
    public async Task<IActionResult> GetTestSecret()
    {
        var secret = await _keyVaultService.GetSecretAsync("SmtpFromEmail");
        
        if (secret == null)
        {
            return NotFound("Secret not found");
        }

        return Ok(new { value = secret });
    }
}
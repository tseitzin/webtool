using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace api.Services;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;

    public KeyVaultService(IConfiguration configuration)
    {
        var vaultUri = configuration["VaultUri"];
        if (string.IsNullOrEmpty(vaultUri))
        {
            throw new InvalidOperationException("VaultUri configuration is missing");
        }

        _secretClient = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
    }

    public async Task<string?> GetSecretAsync(string secretName)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value?.Value;
        }
        catch (Exception)
        {
            // Log the error but don't expose details
            return null;
        }
    }
}
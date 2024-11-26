using Azure.Security.KeyVault.Secrets;

namespace api.Services;

public interface IKeyVaultService
{
    Task<string?> GetSecretAsync(string secretName);
}
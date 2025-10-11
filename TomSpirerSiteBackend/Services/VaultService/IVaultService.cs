using TomSpirerSiteBackend.Models;

namespace TomSpirerSiteBackend.Services.VaultService;

public interface IVaultService
{
    Task<string?> GetSecretAsync(string secretName);
    Task<string?> GetSecretAsync(VaultSecretKey secretName) => GetSecretAsync(secretName.ToString());
}

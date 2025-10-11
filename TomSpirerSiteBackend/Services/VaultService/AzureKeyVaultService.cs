using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using TomSpirerSiteBackend.Services.CacheService;

namespace TomSpirerSiteBackend.Services.VaultService;

public class AzureKeyVaultService : IVaultService
{
    private readonly TimeSpan _defaultCacheExpiration = TimeSpan.FromMinutes(5);
    private readonly SecretClient _secretClient;
    private readonly ILogger<AzureKeyVaultService> _logger;
    private readonly ICacheService _cacheService;

    public AzureKeyVaultService(IConfiguration config, ILogger<AzureKeyVaultService> logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;

        _logger.LogInformation("Initializing Azure Key Vault service...");

        if (string.IsNullOrEmpty(config["AzureKeyVaultUri"]))
        {
            _logger.LogError("KeyVaultUri is required");
            throw new ArgumentException("KeyVaultUri is required");
        }
        if (!Uri.TryCreate(config["AzureKeyVaultUri"], UriKind.Absolute, out Uri? vaultUri))
        {
            _logger.LogError("Invalid KeyVaultUri: {KeyVaultUri}", config["AzureKeyVaultUri"]);
            throw new ArgumentException("Invalid KeyVaultUri");
        }

        // Use Managed Identity (recommended for production)
        TokenCredential credential = new DefaultAzureCredential();

        _secretClient = new SecretClient(vaultUri, credential);

        _logger.LogInformation("Successfully initialized Azure Key Vault service");
    }
    public async Task<string?> GetSecretAsync(string secretName)
    {
        try
        {
            // Generate cache key with prefix to avoid collisions
            var cacheKey = $"keyvault:secret:{secretName}";
        
            // Check cache first
            var cachedValue = _cacheService.Get(cacheKey);
            if (cachedValue != null)
            {
                _logger.LogDebug("Retrieved secret '{SecretName}' from cache", secretName);
                return cachedValue;
            }
            
            _logger.LogInformation("Retrieving secret '{SecretName}' from Key Vault", secretName);
            var response = await _secretClient.GetSecretAsync(secretName);
            _logger.LogInformation("Successfully retrieved secret '{SecretName}' from Key Vault", secretName);
            
            var secretValue = response.Value.Value;
            
            // Cache the secret
            _cacheService.Set(cacheKey, secretValue, _defaultCacheExpiration);
            _logger.LogDebug($"Cached secret '{secretName}' for {_defaultCacheExpiration}");
            
            return secretValue;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Secret '{SecretName}' not found in Key Vault", secretName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret '{SecretName}' from Key Vault", secretName);
            throw;
        }
    }
}

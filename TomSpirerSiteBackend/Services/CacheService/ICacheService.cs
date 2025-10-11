namespace TomSpirerSiteBackend.Services.CacheService;

public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <returns>The cached value or null if not found</returns>
    string? Get(string key);
    
    /// <summary>
    /// Sets a value in the cache with expiration
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">Time until the cache entry expires</param>
    void Set(string key, string value, TimeSpan expiration);
}

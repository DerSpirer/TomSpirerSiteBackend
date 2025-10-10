using System.Collections.Concurrent;

namespace TomSpirerSiteBackend.Services.CacheService;

public class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    
    private class CacheEntry
    {
        public string Value { get; set; }
        public DateTime? ExpiresAt { get; set; }
        
        public CacheEntry(string value, DateTime? expiresAt = null)
        {
            Value = value;
            ExpiresAt = expiresAt;
        }
        
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
    
    public MemoryCacheService(ILogger<MemoryCacheService> logger)
    {
        _logger = logger;
        _cache = new ConcurrentDictionary<string, CacheEntry>();
    }
    
    public string? Get(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                _logger.LogDebug("Cache entry for key '{Key}' has expired and was removed", key);
                return null;
            }
            
            _logger.LogDebug("Cache hit for key '{Key}'", key);
            return entry.Value;
        }
        
        _logger.LogDebug("Cache miss for key '{Key}'", key);
        return null;
    }
    
    public void Set(string key, string value, TimeSpan expiration)
    {
        var expiresAt = DateTime.UtcNow.Add(expiration);
        var entry = new CacheEntry(value, expiresAt);
        _cache.AddOrUpdate(key, entry, (k, v) => entry);
        _logger.LogDebug("Set cache entry for key '{Key}' with expiration at {ExpiresAt}", key, expiresAt);
    }
}

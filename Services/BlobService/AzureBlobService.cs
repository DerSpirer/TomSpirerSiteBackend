using System.Collections.Concurrent;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Services.CacheService;
using TomSpirerSiteBackend.Services.VaultService;

namespace TomSpirerSiteBackend.Services.BlobService;

public class AzureBlobService(ILogger<AzureBlobService> logger, IVaultService vaultService, ICacheService cacheService) : AsyncInitBase(logger), IBlobService
{
    private const string ContainerName = "agent-kb-blobs";
    private const int CacheExpirationMinutes = 60; // Cache blob contents for 1 hour

    private static readonly ConcurrentDictionary<string, BlobContainerClient> _containerClients = new();
    private static BlobServiceClient? _blobServiceClient;

    private readonly ILogger<AzureBlobService> _logger = logger;
    private readonly IVaultService _vaultService = vaultService;
    private readonly ICacheService _cacheService = cacheService;

    protected override async Task InitAsync()
    {
        string? connectionString = await _vaultService.GetSecretAsync(VaultSecretKey.AzureStorageConnectionString);
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Azure Storage Connection String is required");
        }
        _blobServiceClient = new BlobServiceClient(connectionString);
    }
    public async Task<ServiceResult<Stream>> DownloadBlobAsync(string blobName)
    {
        try
        {
            _logger.LogInformation($"Downloading blob: {blobName}");

            // Check cache first
            string cacheKey = GetCacheKey(blobName);
            string? cachedContent = _cacheService.Get(cacheKey);

            if (cachedContent != null)
            {
                _logger.LogInformation($"Retrieved blob from cache: {blobName}");
                byte[] bytes = Convert.FromBase64String(cachedContent);
                return new ServiceResult<Stream> { success = true, data = new MemoryStream(bytes) };
            }

            // Not in cache, download from blob storage
            var containerClient = await GetContainerClient(ContainerName);
            if (containerClient == null)
            {
                _logger.LogError("Failed to get Azure Blob Container Client");
                return new ServiceResult<Stream> { success = false, message = "Failed to get Azure Blob Container Client" };
            }

            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogError($"Blob not found: {blobName}");
                return new ServiceResult<Stream> { success = false, message = $"Blob not found: {blobName}" };
            }

            BlobDownloadInfo response = await blobClient.DownloadAsync();

            // Read the content into memory for caching
            using var memoryStream = new MemoryStream();
            await response.Content.CopyToAsync(memoryStream);
            byte[] contentBytes = memoryStream.ToArray();

            // Cache the content as base64 string
            string base64Content = Convert.ToBase64String(contentBytes);
            _cacheService.Set(cacheKey, base64Content, TimeSpan.FromMinutes(CacheExpirationMinutes));

            _logger.LogInformation($"Successfully downloaded and cached blob: {blobName}");
            return new ServiceResult<Stream> { success = true, data = new MemoryStream(contentBytes) };
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, $"Azure Storage error downloading blob: {blobName}");
            return new ServiceResult<Stream> { success = false, message = $"Failed to download blob: {ex.Message}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading blob: {blobName}");
            return new ServiceResult<Stream> { success = false, message = $"An error occurred: {ex.Message}" };
        }
    }

    public async Task<ServiceResult<List<string>>> ListBlobsAsync(string? prefix = null)
    {
        try
        {
            _logger.LogInformation($"Listing blobs with prefix: {prefix ?? "none"}");

            var containerClient = await GetContainerClient(ContainerName);
            if (containerClient == null)
            {
                _logger.LogError("Failed to get Azure Blob Container Client");
                return new ServiceResult<List<string>> { success = false, message = "Failed to get Azure Blob Container Client" };
            }

            var blobNames = new List<string>();
            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                blobNames.Add(blobItem.Name);
            }

            _logger.LogInformation($"Successfully listed {blobNames.Count} blobs with prefix: {prefix ?? "none"}");
            return new ServiceResult<List<string>> { success = true, data = blobNames };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error listing blobs");
            return new ServiceResult<List<string>> { success = false, message = $"An error occurred: {exception.Message}" };
        }
    }
    public async Task<ServiceResult<string>> ReadPromptBlobAsync()
    {
        const string promptBlobName = "Prompt.md";
        var result = await DownloadBlobAsync(promptBlobName);
        if (!result.success || result.data == null)
        {
            return new ServiceResult<string> { success = false, message = result.message ?? "Failed to download prompt blob" };
        }
        using var reader = new StreamReader(result.data);
        string content = await reader.ReadToEndAsync();

        return new ServiceResult<string> { success = true, data = content };
    }
    private async Task<BlobContainerClient> GetContainerClient(string containerName = ContainerName)
    {
        await AwaitInitAsync();
        // Create a container client if it is not already created
        if (!_containerClients.TryGetValue(containerName, out BlobContainerClient? containerClient))
        {
            containerClient = _blobServiceClient!.GetBlobContainerClient(containerName);
            _containerClients.TryAdd(containerName, containerClient);
        }

        // Return the container client
        return containerClient;
    }
    private string GetCacheKey(string blobName) => $"blob:{blobName}";
}


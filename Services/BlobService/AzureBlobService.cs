using System.Collections.Concurrent;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Services.VaultService;

namespace TomSpirerSiteBackend.Services.BlobService;

public class AzureBlobService : IBlobService
{
    private static ConcurrentDictionary<string, BlobContainerClient> _containerClients = new();
    private static BlobServiceClient? _blobServiceClient;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

    private readonly ILogger<AzureBlobService> _logger;
    private readonly IVaultService _vaultService;

    public AzureBlobService(ILogger<AzureBlobService> logger, IVaultService vaultService)
    {
        _logger = logger;
        _vaultService = vaultService;
    }

    public async Task<ServiceResult<Stream>> DownloadBlobAsync(string containerName, string blobName)
    {
        try
        {
            var containerClient = await GetContainerClient(containerName);
            if (containerClient == null)
            {
                return new ServiceResult<Stream> { success = false, message = "Failed to get Azure Blob Container Client" };
            }

            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning($"Blob not found: {blobName}");
                return new ServiceResult<Stream> { success = false, message = $"Blob not found: {blobName}" };
            }

            BlobDownloadInfo response = await blobClient.DownloadAsync();
            _logger.LogInformation($"Successfully downloaded blob: {blobName}");
            return new ServiceResult<Stream> { success = true, data = response.Content };
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

    public async Task<ServiceResult<List<string>>> ListBlobsAsync(string containerName, string? prefix = null)
    {
        try
        {
            var containerClient = await GetContainerClient(containerName);
            if (containerClient == null)
            {
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
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Storage error listing blobs");
            return new ServiceResult<List<string>> { success = false, message = $"Failed to list blobs: {ex.Message}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs");
            return new ServiceResult<List<string>> { success = false, message = $"An error occurred: {ex.Message}" };
        }
    }
    private async Task<BlobContainerClient?> GetContainerClient(string containerName)
    {
        // Initialize the blob service client if it is not already initialized
        // Use a semaphore to ensure that only one thread initializes the blob service client
        if (_blobServiceClient == null)
        {
            await _initSemaphore.WaitAsync();
            try
            {
                if (_blobServiceClient == null)
                {
                    string? connectionString = await _vaultService.GetSecretAsync(VaultSecretKey.AzureStorageConnectionString);
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return null;
                    }
                    _blobServiceClient = new BlobServiceClient(connectionString);
                }
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        // Create a container client if it is not already created
        if (!_containerClients.TryGetValue(containerName, out BlobContainerClient? containerClient))
        {
            containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            _containerClients.TryAdd(containerName, containerClient);
        }

        // Return the container client
        return containerClient;
    }
}


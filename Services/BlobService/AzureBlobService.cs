using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.Config;

namespace TomSpirerSiteBackend.Services.BlobService;

public class AzureBlobService : IBlobService
{
    private readonly ILogger<AzureBlobService> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobStorageSettings _settings;

    public AzureBlobService(ILogger<AzureBlobService> logger, IOptions<BlobStorageSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        
        _blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
    }

    public async Task<ServiceResult<Stream>> DownloadBlobAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            
            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning($"Blob not found: {blobName}");
                return new ServiceResult<Stream> { success = false, message = $"Blob not found: {blobName}" };
            }

            var response = await blobClient.DownloadAsync();
            _logger.LogInformation($"Successfully downloaded blob: {blobName}");
            return new ServiceResult<Stream> { success = true, data = response.Value.Content };
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
            var blobNames = new List<string>();
            
            await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: prefix))
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
}


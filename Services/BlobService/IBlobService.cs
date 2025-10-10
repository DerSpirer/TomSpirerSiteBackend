using TomSpirerSiteBackend.Models;

namespace TomSpirerSiteBackend.Services.BlobService;

public interface IBlobService
{
    Task<ServiceResult<Stream>> DownloadBlobAsync(string containerName, string blobName);
    Task<ServiceResult<List<string>>> ListBlobsAsync(string containerName, string? prefix = null);
}


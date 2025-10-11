using TomSpirerSiteBackend.Models;

namespace TomSpirerSiteBackend.Services.BlobService;

public interface IBlobService
{
    Task<ServiceResult<Stream>> DownloadBlobAsync(string blobName);
    Task<ServiceResult<List<string>>> ListBlobsAsync(string? prefix = null);
    Task<ServiceResult<string>> ReadPromptBlobAsync();
}


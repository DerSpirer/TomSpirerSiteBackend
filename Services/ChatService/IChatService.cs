using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;

namespace TomSpirerSiteBackend.Services.ChatService;

public interface IChatService
{
    IAsyncEnumerable<string> CreateResponseStream(GenerateResponseRequest request, CancellationToken cancellationToken);
}
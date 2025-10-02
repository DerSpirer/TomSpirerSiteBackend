using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;

namespace TomSpirerSiteBackend.Services.ChatService;

public interface IChatService
{
    Task<ServiceResult<Message>> CreateResponse(GenerateResponseRequest request);
    IAsyncEnumerable<string> CreateResponseStream(GenerateResponseRequest request, CancellationToken cancellationToken);
}
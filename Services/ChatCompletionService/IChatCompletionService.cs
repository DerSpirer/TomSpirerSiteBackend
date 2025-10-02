using TomSpirerSiteBackend.Models;

namespace TomSpirerSiteBackend.Services.ChatCompletionService;

public interface IChatCompletionService
{
    Task<ServiceResult<Message>> GenerateResponse(List<Message> messages);
    IAsyncEnumerable<string> CreateResponseStream(List<Message> messages, CancellationToken cancellationToken);
}
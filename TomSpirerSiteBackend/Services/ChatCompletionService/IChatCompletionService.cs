using TomSpirerSiteBackend.Models;

namespace TomSpirerSiteBackend.Services.ChatCompletionService;

public interface IChatCompletionService
{
    IAsyncEnumerable<Message> CreateResponseStream(IEnumerable<Message> messages, IEnumerable<FunctionTool>? tools, CancellationToken cancellationToken);
}
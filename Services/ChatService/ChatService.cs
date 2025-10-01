namespace TomSpirerSiteBackend.Services.ChatService;

using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;
using TomSpirerSiteBackend.Services.ChatCompletionService;

public class ChatService : IChatService
{
    private readonly IChatCompletionService _chatCompletionService;

    public ChatService(IChatCompletionService chatCompletionService)
    {
        _chatCompletionService = chatCompletionService;
    }

    public async Task<ServiceResult<Message>> GenerateResponse(GenerateResponseRequest request)
    {
        return await _chatCompletionService.GenerateResponse(request.messages);
    }
}
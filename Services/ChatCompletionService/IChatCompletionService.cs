using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;

namespace TomSpirerSiteBackend.Services.ChatCompletionService;

public interface IChatCompletionService
{
    Task<ServiceResult<Message>> GenerateResponse(List<Message> messages);
}
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;

namespace TomSpirerSiteBackend.Services.ChatService;

public interface IChatService
{
    Task<ServiceResult<Message>> GenerateResponse(GenerateResponseRequest request);
}
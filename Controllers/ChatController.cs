using Microsoft.AspNetCore.Mvc;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;
using TomSpirerSiteBackend.Services.ChatService;

namespace TomSpirerSiteBackend.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly IChatService _chatService;
    public ChatController(ILogger<ChatController> logger, IChatService chatService)
    {
        _logger = logger;
        _chatService = chatService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServiceResult<Message>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateResponse([FromBody] GenerateResponseRequest request)
    {
        try
        {
            ServiceResult<Message> result = await _chatService.GenerateResponse(request);
            return result.success ? Ok(result) : BadRequest(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error generating response");
            return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResult<Message> { success = false, message = "Error generating response" });
        }
    }
}
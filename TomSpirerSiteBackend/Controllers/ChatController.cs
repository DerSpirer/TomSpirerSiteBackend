using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;
using TomSpirerSiteBackend.Services.ChatService;
using TomSpirerSiteBackend.Services.EmailService;

namespace TomSpirerSiteBackend.Controllers;

[EnableCors]
[ApiController]
[Route("api/[controller]/[action]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly IChatService _chatService;
    private readonly IEmailService _emailService;
    public ChatController(ILogger<ChatController> logger, IChatService chatService, IEmailService emailService)
    {
        _logger = logger;
        _chatService = chatService;
        _emailService = emailService;
    }
    
    private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
    };
    [HttpPost]
    public async Task GenerateResponse([FromBody] GenerateResponseRequest request, CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        try
        {
            await foreach (Message delta in _chatService.CreateResponseStream(request, cancellationToken))
            {
                string chunkJson = JsonConvert.SerializeObject(delta, _jsonSettings);
                byte[] chunkBytes = Encoding.UTF8.GetBytes($"data: {chunkJson}\n\n");
                await Response.Body.WriteAsync(chunkBytes, 0, chunkBytes.Length, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error generating response");
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LeaveMessage([FromBody] LeaveMessageToolParams payload)
    {
        try
        {
            bool success = await _emailService.LeaveMessage(payload);
            return success ? Ok() : BadRequest(new { error = "Failed to leave message" });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error leaving message");
            return StatusCode(500, "Internal server error");
        }
    }
}
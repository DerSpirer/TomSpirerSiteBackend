using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    public async Task GenerateResponse([FromBody] GenerateResponseRequest request, CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        try
        {
            await foreach (string chunk in _chatService.CreateResponseStream(request, cancellationToken))
            {
                string chunkJson = JsonConvert.SerializeObject(new { delta = chunk });
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
}
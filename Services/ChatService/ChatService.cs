using System.Runtime.CompilerServices;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;
using TomSpirerSiteBackend.Services.BlobService;
using TomSpirerSiteBackend.Services.ChatCompletionService;

namespace TomSpirerSiteBackend.Services.ChatService;

public class ChatService(IChatCompletionService chatCompletionService, IBlobService blobService, ILogger<ChatService> logger) : IChatService
{
    private readonly IChatCompletionService _chatCompletionService = chatCompletionService;
    private readonly IBlobService _blobService = blobService;
    private readonly ILogger<ChatService> _logger = logger;
    
    public async IAsyncEnumerable<Message> CreateResponseStream(GenerateResponseRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Creating response stream for request with {request.messages.Count} messages");
        
        ServiceResult<string> systemMessage = await _blobService.ReadPromptBlobAsync();
        if (!systemMessage.success || string.IsNullOrEmpty(systemMessage.data))
            throw new Exception($"Failed to read prompt blob: {systemMessage.message}");
        List<Message> messages =
        [
            new()
            {
                role = Message.Role.system,
                content = systemMessage.data
            },
            ..request.messages
        ];
        
        FunctionTool leaveMessageTool = new FunctionTool
        {
            name = "leave_message",
            description = @"Use this tool to leave a message for Tom.
If any information is missing, ask for it from the user.
ASK FOR ONLY A SINGLE piece of information in your response message.
Then the user will respond, and then you may ask for the next piece of information.",
            parameterType = typeof(LeaveMessageToolParams)
        };
        List<FunctionTool> tools =
        [
            leaveMessageTool,
        ];
        await foreach (Message delta in _chatCompletionService.CreateResponseStream(messages, tools, cancellationToken))
        {
            yield return delta;
        }
        
        _logger.LogInformation($"Completed response stream for request");
    }
}
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.Config;
using TomSpirerSiteBackend.Models.DTOs;
using TomSpirerSiteBackend.Services.ChatCompletionService;

namespace TomSpirerSiteBackend.Services.ChatService;

public class ChatService : IChatService
{
    private readonly IChatCompletionService _chatCompletionService;
    private readonly AgentSettings _agentSettings;
    
    public ChatService(IChatCompletionService chatCompletionService, IOptions<AgentSettings> agentSettings)
    {
        _chatCompletionService = chatCompletionService;
        _agentSettings = agentSettings.Value;
    }

    public async IAsyncEnumerable<Message> CreateResponseStream(GenerateResponseRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<Message> messages =
        [
            new Message
            {
                role = Message.Role.system,
                content = 
$@"{_agentSettings.Instructions}

```
{_agentSettings.ProfessionalSummary}
```
"
            },
            ..request.messages
        ];
        
        FunctionTool leaveMessageTool = new FunctionTool
        {
            name = "leave_message",
            description = @"Use this tool to leave a message for Tom.
If any information is missing, ask for it from the user.
ASK FOR ONLY A SINGLE piece of information in your response message. Then the user will respond, and then you may ask for the next piece of information.",
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
    }
}
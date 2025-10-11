using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TomSpirerSiteBackend.Models.OpenAI;

public class OpenAiChatCompletionChunk
{
    public FinishReason? finish_reason { get; set; }
    public List<Choice> choices { get; set; } = new();

    public class Choice
    {
        public FinishReason? finish_reason { get; set; }
        public Message delta { get; set; } = new();
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FinishReason
    {
        stop,
        length,
        content_filter,
        tool_calls,
    }
}


using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TomSpirerSiteBackend.Utils;

namespace TomSpirerSiteBackend.Models;

public class Message
{
    public Role? role { get; set; }
    public string? content { get; set; }
    public string? refusal { get; set; }
    public List<ToolCall>? toolCalls { get; set; }
    public string? toolCallId { get; set; }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Role
    {
        system,
        user,
        assistant,
        tool,
        developer,
    }

    public class ToolCall
    {
        public string type { get; set; } = "function";
        public string id { get; set; }
        public FunctionCall function { get; set; }

        public class FunctionCall
        {
            public string name { get; set; }
            [JsonProperty("arguments")]
            public string argumentsJson { get; set; }
            
            public T? GetArgumentsAs<T>()
            {
                return Helpers.TryDeserialize<T>(argumentsJson);
            }
        }
    }
}
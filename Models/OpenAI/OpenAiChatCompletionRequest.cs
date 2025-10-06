using Newtonsoft.Json.Linq;
using TomSpirerSiteBackend.Utils;

namespace TomSpirerSiteBackend.Models.OpenAI;

public class OpenAiChatCompletionRequest
{
    public required List<Message> messages { get; set; }
    public string model { get; set; } = "gpt-4o";
    public List<Tool>? tools { get; set; } = null;
    public float temperature { get; set; } = 0.01f;
    public bool stream { get; set; } = true;

    public class Tool
    {
        public string type { get; set; } = "function";
        public Function function { get; set; }
        public class Function
        {
            public string name { get; set; }
            public string description { get; set; }
            public JObject parameters { get; set; }
        }

        public static Tool FromFunctionTool(FunctionTool functionTool) => new()
        {
            function = new()
            {
                name = functionTool.name,
                description = functionTool.description,
                parameters = Helpers.GenerateOpenAiParametersSchema(functionTool.parameterType)
            }
        };
    }
}


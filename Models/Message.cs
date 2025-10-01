using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TomSpirerSiteBackend.Models;

public class Message
{
    public Role role { get; set; }
    public string content { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Role
    {
        system,
        user,
        assistant,
        tool,
        developer,
    }
}
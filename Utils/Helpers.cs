using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TomSpirerSiteBackend.Utils;

public static class Helpers
{
    private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        ContractResolver = new DefaultContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };
    public static T? TryDeserialize<T>(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
        }
        catch (Exception)
        {
            return default;
        }
    }
}
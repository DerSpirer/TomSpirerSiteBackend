using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
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

    public static JSchema GenerateJsonSchema(Type type)
    {
        JSchemaGenerator generator = new JSchemaGenerator
        {
            // Use camelCase for property names to match JSON conventions
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DefaultRequired = Required.DisallowNull
        };

        return generator.Generate(type);
    }
    public static JSchema GenerateJsonSchema<T>() => GenerateJsonSchema(typeof(T));
    public static JObject GenerateOpenAiParametersSchema(Type type)
    {
        JObject parameters = new();
        if (type == null)
        {
            parameters["type"] = "object";
            parameters["properties"] = new JObject();
            parameters["required"] = new JArray();
        }
        else
        {
            JSchema schema = GenerateJsonSchema(type);
            parameters = JObject.Parse(schema.ToString());   
        }
        if (parameters["type"]?.ToString() != "object")
        {
            throw new InvalidOperationException("Parameter type must be a class or object. OpenAI function parameters require an object type.");
        }
        SetAdditionalPropertiesFalse(parameters);
        return parameters;
    }
    public static JObject GenerateOpenAiParametersSchema<T>() => GenerateOpenAiParametersSchema(typeof(T));
    private static void SetAdditionalPropertiesFalse(JObject obj)
    {
        if (obj["type"]?.ToString() == "object")
        {
            obj["additionalProperties"] = false;
            if (obj["properties"] is JObject properties)
            {
                foreach (var property in properties.Properties())
                {
                    if (property.Value is JObject propertyObj)
                    {
                        SetAdditionalPropertiesFalse(propertyObj);
                    }
                }
            }
        }
        else if (obj["type"]?.ToString() == "array" && obj["items"] is JObject items)
        {
            SetAdditionalPropertiesFalse(items);
        }
    }
}
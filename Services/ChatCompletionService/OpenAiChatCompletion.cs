using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.Config;
using TomSpirerSiteBackend.Utils;

namespace TomSpirerSiteBackend.Services.ChatCompletionService;

using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public class OpenAiChatCompletion : IChatCompletionService
{
    private readonly ILogger<OpenAiChatCompletion> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.openai.com/v1";
    private readonly string _apiKey;

    public OpenAiChatCompletion(ILogger<OpenAiChatCompletion> logger, HttpClient httpClient, IOptions<OpenAiSettings> openAiSettings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _apiKey = openAiSettings.Value.ApiKey;
    }

    public async Task<ServiceResult<Message>> GenerateResponse(List<Message> messages)
    {
        ServiceResult<Message> result = new ServiceResult<Message>();

        try
        {
            Uri uri = new Uri($"{_baseUrl}/chat/completions");
            OpenAiChatCompletionRequest request = new OpenAiChatCompletionRequest { messages = messages };
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"),
                Headers = {
                    Authorization = new AuthenticationHeaderValue("Bearer", _apiKey)
                }
            };

            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                result.message = "Chat completion request to OpenAI failed";
                _logger.LogError(responseBody, result.message);
                return result;
            }

            OpenAiChatCompletionResponse? responseObject = Helpers.TryDeserialize<OpenAiChatCompletionResponse?>(responseBody);
            if (responseObject == null)
            {
                result.message = "Failed to deserialize OpenAI response";
                _logger.LogError(responseBody, result.message);
                return result;
            }

            result.success = true;
            result.data = responseObject.choices[0].message;
            return result;
        }
        catch (Exception exception)
        {
            result.message = "Unexpected error generating response";
            _logger.LogError(exception, result.message);
            return result;
        }
    }
    public async IAsyncEnumerable<string> CreateResponseStream(List<Message> messages, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;
        try
        {
            Uri uri = new Uri($"{_baseUrl}/chat/completions");
            OpenAiChatCompletionRequest request = new OpenAiChatCompletionRequest { messages = messages, stream = true };
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"),
                Headers = {
                    Authorization = new AuthenticationHeaderValue("Bearer", _apiKey)
                }
            };

            response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(responseBody, "Chat completion request to OpenAI failed");
                yield break;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error creating response stream");
            yield break;
        }

        using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using StreamReader reader = new StreamReader(responseStream);

        Console.WriteLine("Starting to read stream...");
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            string? line = await reader.ReadLineAsync();
            Console.WriteLine(line);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            string data = line["data: ".Length..];
            if (data == "[DONE]")
            {
                Console.WriteLine("Stream ended.");
                yield break;
            }

            OpenAiChatCompletionStreamResponse? responseObject = Helpers.TryDeserialize<OpenAiChatCompletionStreamResponse?>(data);
            if (responseObject == null)
                continue;

            string? content = responseObject.GetContent();
            if (string.IsNullOrWhiteSpace(content))
                continue;

            yield return content;
        }
    }

    private class OpenAiChatCompletionRequest
    {
        public string model { get; set; } = "gpt-4o";
        public List<Message> messages { get; set; }
        public float temperature { get; set; } = 0.01f;
        public bool stream { get; set; }
    }

    private class OpenAiChatCompletionResponse
    {
        public string id { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public List<Choice> choices { get; set; }

        public class Choice
        {
            public Message message { get; set; }
        }
    }

    private class OpenAiChatCompletionStreamResponse
    {
        public List<Choice> choices { get; set; }
        public class Choice
        {
            public Delta delta { get; set; }

            public class Delta
            {
                public string content { get; set; }
            }
        }
        public string GetContent() => choices[0].delta.content;
    }
}
namespace TomSpirerSiteBackend.Services.ChatCompletionService;

using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;
using TomSpirerSiteBackend.Models.Config;
using TomSpirerSiteBackend.Utils;

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

    private class OpenAiChatCompletionRequest
    {
        public string model { get; set; } = "gpt-4o-mini";
        public List<Message> messages { get; set; }
    }

    private class OpenAiChatCompletionResponse
    {
        public string id { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public List<Choice> choices { get; set; }

        public struct Choice
        {
            public Message message { get; set; }
        }
    }
}
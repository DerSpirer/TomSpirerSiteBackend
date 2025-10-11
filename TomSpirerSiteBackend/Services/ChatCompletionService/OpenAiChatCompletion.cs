using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.OpenAI;
using TomSpirerSiteBackend.Utils;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TomSpirerSiteBackend.Services.VaultService;

namespace TomSpirerSiteBackend.Services.ChatCompletionService;

public class OpenAiChatCompletion : IChatCompletionService
{
    private const string EP_URL = "https://api.openai.com/v1";
    private readonly ILogger<OpenAiChatCompletion> _logger;
    private readonly HttpClient _httpClient;
    private readonly IVaultService _vaultService;
    private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        }
    };

    public OpenAiChatCompletion(ILogger<OpenAiChatCompletion> logger, HttpClient httpClient, IVaultService vaultService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _vaultService = vaultService;
    }

    public async IAsyncEnumerable<Message> CreateResponseStream(IEnumerable<Message> messages, IEnumerable<FunctionTool>? tools, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = await SendRequest(messages, tools, cancellationToken);
        if (response == null || !response.IsSuccessStatusCode)
            yield break;
        await foreach (Message delta in ReadResponseStream(response, cancellationToken))
        {
            yield return delta;
        }
    }
    private async Task<HttpResponseMessage?> SendRequest(IEnumerable<Message> messages, IEnumerable<FunctionTool>? tools, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;
        try
        {
            Uri uri = new Uri($"{EP_URL}/chat/completions");
            OpenAiChatCompletionRequest request = new OpenAiChatCompletionRequest
            {
                messages = messages.ToList(),
                tools = tools.Select(OpenAiChatCompletionRequest.Tool.FromFunctionTool).ToList(),
            };
            
            string? apiKey = await _vaultService.GetSecretAsync(VaultSecretKey.OpenAiApiKey);
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("OpenAI API key is not configured");
                throw new InvalidOperationException("OpenAI API key is not configured");
            }
            
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonConvert.SerializeObject(request, _jsonSettings), Encoding.UTF8, "application/json"),
                Headers = {
                    Authorization = new AuthenticationHeaderValue("Bearer", apiKey)
                }
            };

            response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(responseBody, "Chat completion request to OpenAI failed");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error creating response stream");
        }
        return response;
    }
    private async IAsyncEnumerable<Message> ReadResponseStream(HttpResponseMessage response, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using StreamReader reader = new StreamReader(responseStream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            string? line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            string data = line["data: ".Length..];
            if (data == "[DONE]")
                yield break;

            OpenAiChatCompletionChunk? responseObject = Helpers.TryDeserialize<OpenAiChatCompletionChunk?>(data);
            if (responseObject == null)
                continue;
            
            yield return responseObject.choices[0].delta;
        }
    }
}
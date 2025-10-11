using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServerlessFunctions.Functions;

public class AgentKbBlobTrigger
{
    private readonly ILogger<AgentKbBlobTrigger> _logger;

    public AgentKbBlobTrigger(ILogger<AgentKbBlobTrigger> logger)
    {
        _logger = logger;
    }

    [Function(nameof(AgentKbBlobTrigger))]
    public async Task Run([BlobTrigger("agent-kb-blobs/{name}", Connection = "StorageConnectionString")] Stream stream, string name)
    {
        using var blobStreamReader = new StreamReader(stream);
        var content = await blobStreamReader.ReadToEndAsync();
        _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        
    }
}
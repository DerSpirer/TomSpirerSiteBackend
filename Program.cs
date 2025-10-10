using Newtonsoft.Json.Schema;
using Scalar.AspNetCore;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.Config;
using TomSpirerSiteBackend.Services.BlobService;
using TomSpirerSiteBackend.Services.ChatCompletionService;
using TomSpirerSiteBackend.Services.ChatService;
using TomSpirerSiteBackend.Services.EmailService;
using TomSpirerSiteBackend.Services.VaultService;
using TomSpirerSiteBackend.Services.CacheService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGitHub", policy =>
    {
        policy.WithOrigins("https://derspirer.github.io", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMvc().AddNewtonsoftJson();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IChatCompletionService, OpenAiChatCompletion>();

builder.Services.Configure<AgentSettings>(builder.Configuration.GetSection("AgentSettings"));

builder.Services.AddSingleton<IVaultService, AzureKeyVaultService>();
builder.Services.AddSingleton<IChatCompletionService, OpenAiChatCompletion>();
builder.Services.AddSingleton<IEmailService, GmailService>();
builder.Services.AddSingleton<IBlobService, AzureBlobService>();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

IVaultService vaultService = app.Services.GetRequiredService<IVaultService>();
string? licenseKey = await vaultService.GetSecretAsync(VaultSecretKey.JsonSchemaLicenseKey);
if (!string.IsNullOrEmpty(licenseKey))
{
    License.RegisterLicense(licenseKey);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowGitHub");
app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();
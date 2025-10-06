using Newtonsoft.Json.Schema;
using Scalar.AspNetCore;
using TomSpirerSiteBackend.Models.Config;
using TomSpirerSiteBackend.Services.ChatCompletionService;
using TomSpirerSiteBackend.Services.ChatService;
using TomSpirerSiteBackend.Services.EmailService;

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
string jsonSchemaLicenseKey = builder.Configuration.GetSection("OpenAi")["JsonSchemaLicenseKey"] ?? throw new InvalidOperationException("JsonSchemaLicenseKey is not configured.");
License.RegisterLicense(jsonSchemaLicenseKey);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAi"));
builder.Services.Configure<PromptSettings>(builder.Configuration.GetSection("Prompt"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddHttpClient<IChatCompletionService, OpenAiChatCompletion>();

builder.Services.AddSingleton<IChatCompletionService, OpenAiChatCompletion>();
builder.Services.AddSingleton<IEmailService, GmailService>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowGitHub");
app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();
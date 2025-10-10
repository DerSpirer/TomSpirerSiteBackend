using System.Net;
using System.Net.Mail;
using Newtonsoft.Json;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Services.VaultService;

namespace TomSpirerSiteBackend.Services.EmailService;

public class GmailService(ILogger<GmailService> logger, IVaultService vaultService) : AsyncInitBase(logger), IEmailService
{
    private readonly ILogger<GmailService> _logger = logger;
    private readonly IVaultService _vaultService = vaultService;
    private SmtpClient? _smtpClient;
    private string? _inboxEmail;

    protected override async Task InitAsync()
    {
        _inboxEmail = await _vaultService.GetSecretAsync(VaultSecretKey.GmailInboxEmail);
        string? appPassword = await _vaultService.GetSecretAsync(VaultSecretKey.GmailAppPassword);
        _smtpClient = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_inboxEmail, appPassword)
        };
    }
    public async Task<bool> LeaveMessage(LeaveMessageToolParams messageParams)
    {
        bool success = false;

        try
        {
            await AwaitInitAsync();
            _logger.LogInformation($"Leaving message with parameters: {JsonConvert.SerializeObject(messageParams)}");
            using MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(_inboxEmail!),
                To = { new MailAddress(_inboxEmail!) },
                Subject = $"New message from {messageParams.fromName} - {messageParams.subject}",
                Body = @$"You have received a new message from {messageParams.fromName} ({messageParams.fromEmail}):

Subject:
{messageParams.subject}

Message:
{messageParams.body}",
                IsBodyHtml = false,
            };
            await _smtpClient!.SendMailAsync(mailMessage);
            success = true;
            _logger.LogInformation($"Email sent successfully with parameters: {JsonConvert.SerializeObject(messageParams)}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send email");
        }

        return success;
    }
    
    public override void Dispose()
    {
        _smtpClient?.Dispose();
        _smtpClient = null;
        _inboxEmail = null;
        base.Dispose();
    }
    public override async ValueTask DisposeAsync()
    {
        _smtpClient?.Dispose();
        _smtpClient = null;
        _inboxEmail = null;
        await base.DisposeAsync();
    }
}
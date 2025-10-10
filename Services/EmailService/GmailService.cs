using System.Net;
using System.Net.Mail;
using Newtonsoft.Json;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Services.VaultService;

namespace TomSpirerSiteBackend.Services.EmailService;

public class GmailService : IEmailService
{
    private readonly ILogger<GmailService> _logger;
    private readonly IVaultService _vaultService;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private SmtpClient? _smtpClient;
    private string? _inboxEmail;

    public GmailService(ILogger<GmailService> logger, IVaultService vaultService)
    {
        _logger = logger;
        _vaultService = vaultService;
    }
    public async Task<bool> LeaveMessage(LeaveMessageToolParams messageParams)
    {
        bool success = false;

        try
        {
            SmtpClient? smtpClient = await GetSmtpClient();
            if (smtpClient == null || string.IsNullOrEmpty(_inboxEmail))
            {
                _logger.LogError("Failed to get SMTP client");
                return false;
            }

            using MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(_inboxEmail),
                To = { new MailAddress(_inboxEmail) },
                Subject = $"New message from {messageParams.fromName} - {messageParams.subject}",
                Body = @$"You have received a new message from {messageParams.fromName} ({messageParams.fromEmail}):

Subject:
{messageParams.subject}

Message:
{messageParams.body}",
                IsBodyHtml = false,
            };
            await smtpClient.SendMailAsync(mailMessage);
            success = true;
            _logger.LogInformation($"Email sent successfully with parameters: {JsonConvert.SerializeObject(messageParams)}");
        }
        catch (Exception)
        {
            _logger.LogError("Failed to send email");
        }

        return success;
    }
    private async Task<SmtpClient?> GetSmtpClient()
    {
        if (_smtpClient == null)
        {
            await _initSemaphore.WaitAsync();
            try
            {
                if (_smtpClient == null)
                {
                    _inboxEmail = await _vaultService.GetSecretAsync(VaultSecretKey.GmailInboxEmail);
                    string? appPassword = await _vaultService.GetSecretAsync(VaultSecretKey.GmailAppPassword);
                    _smtpClient = new SmtpClient("smtp.gmail.com", 587)
                    {
                        EnableSsl = true,
                        Credentials = new NetworkCredential(_inboxEmail, appPassword)
                    };
                }
            }
            finally
            {
                _initSemaphore.Release();
            }
        }
        return _smtpClient;
    }
}
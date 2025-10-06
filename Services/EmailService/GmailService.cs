using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.Config;

namespace TomSpirerSiteBackend.Services.EmailService;

public class GmailService : IEmailService
{
    private readonly ILogger<GmailService> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly SmtpClient _smtpClient;

    public GmailService(ILogger<GmailService> logger, IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
        _smtpClient = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_emailSettings.InboxEmail, _emailSettings.AppPassword)
        };
    }
    public async Task<bool> LeaveMessage(LeaveMessageToolParams messageParams)
    {
        bool success = false;

        try
        {
            using MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.InboxEmail),
                To = { new MailAddress(_emailSettings.InboxEmail) },
                Subject = $"New message from {messageParams.fromName} - {messageParams.subject}",
                Body = @$"You have received a new message from {messageParams.fromName} ({messageParams.fromEmail}):

Subject:
{messageParams.subject}

Message:
{messageParams.body}",
                IsBodyHtml = false,
            };
            await _smtpClient.SendMailAsync(mailMessage);
            success = true;
            _logger.LogInformation($"Email sent successfully with parameters: {JsonConvert.SerializeObject(messageParams)}");
        }
        catch (Exception)
        {
            _logger.LogError("Failed to send email");
        }
        
        return success;
    }
}
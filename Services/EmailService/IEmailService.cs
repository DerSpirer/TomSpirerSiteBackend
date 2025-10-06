using TomSpirerSiteBackend.Models;

namespace TomSpirerSiteBackend.Services.EmailService;

public interface IEmailService
{
    Task<bool> LeaveMessage(LeaveMessageToolParams messageParams);
}
using System;
namespace FunctionalService
{
	public interface IFunctionalSvc
	{
        Task CreateDefaultAdminUser();
        Task CreateDefaultUser();
        Task SendEmailByGmailAsync(string? fromEmail, string? fromFullName, string subject, string strMessageBody, string email1, string email2, string? smtpUsername, string? smtpPassword, string? smtpHost, int smtpPort, bool smtpSSl);
        Task SendEmailBySendGridAsync(string? sendGridKey, string? fromEmail, string? fromFullName, string subject, string strMessageBody, string email);
    }
}


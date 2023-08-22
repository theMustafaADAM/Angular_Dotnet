using System;
using FunctionalService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using ModelService;
using Serilog;

namespace EmailService
{
    public class EmailSvc : IEmailSvc
	{
        [Obsolete]
        private readonly IHostingEnvironment _env;
		private readonly SendGridOptions _sendGridOptions;
		private readonly IFunctionalSvc _functionalSvc;
		private readonly SmtpOptions _smtpOptions;

        [Obsolete]
        public EmailSvc(
			IOptions<SendGridOptions> sendGridOptions,
			IOptions<SmtpOptions> smtpOptions,
			IFunctionalSvc functionalSvc,
			IHostingEnvironment env
		)
		{
			_env = env;
			_functionalSvc = functionalSvc;
			_smtpOptions = smtpOptions.Value;
			_sendGridOptions = sendGridOptions.Value;
		}

        [Obsolete]
        public Task SendEmailAsync(string email, string subject, string message, string template)
		{
			var strMessageBody = BuildingEmailBody(message, template, subject);

			// Check if default emails sending options from app setting
			if (_sendGridOptions.IsDefault)
			{
				_functionalSvc.SendEmailBySendGridAsync(
					_sendGridOptions.SendGridKey,
					_sendGridOptions.FromEmail,
					_sendGridOptions.FromFullName,
					subject, strMessageBody, email).Wait();
			}

			if (!_smtpOptions.IsDefault) return Task.CompletedTask;

			if (!string.IsNullOrEmpty(strMessageBody))
			{
				// Then we need to send email using SMTP
				_functionalSvc.SendEmailByGmailAsync(
					_smtpOptions.FromEmail,
					_smtpOptions.FromFullName,
					subject, strMessageBody, email, email,
					_smtpOptions.SmtpUsername,
					_smtpOptions.SmtpPassword,
					_smtpOptions.SmtpHost,
					_smtpOptions.SmtpPort,
                    _smtpOptions.SmtpSSl).Wait();
			}

			return Task.CompletedTask;
		}

        [Obsolete]
        private string BuildingEmailBody(string message, string templateName, string subject)
		{
			var strMessage = string.Empty;

			try
			{
				var strTemplateFilePath = _env.ContentRootPath = "/EmailTemplates/" + templateName;
				var reader = new StreamReader(strTemplateFilePath);
				strMessage = reader.ReadToEnd();
				reader.Close();
			}
			catch (Exception ex)
			{
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
					ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
				strMessage = string.Empty;
            }

			strMessage = strMessage.Replace("[[[Title]]]", string.IsNullOrEmpty(subject) ? "Notification => techhowdy.com" : subject);
			strMessage = strMessage.Replace("[[[message]]]", message);
            return strMessage;
		}
	}
}


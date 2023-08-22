using System;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ModelService;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;

namespace FunctionalService
{
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8604 // Possible null reference argument.

    public class FunctionalSvc : IFunctionalSvc
	{
        [Obsolete]
        private readonly IHostingEnvironment _env;
		private readonly AppUserOptions _appUserOptions;
		private readonly AdminUserOptions _adminUserOptions;
		private readonly UserManager<ApplicationUser> _userManager;

        [Obsolete]
        public FunctionalSvc(
			IOptions<AdminUserOptions> adminUserOptions,
			UserManager<ApplicationUser> userManager,
			IOptions<AppUserOptions> appUserOptions,
			IHostingEnvironment env
		)
		{
			_adminUserOptions = adminUserOptions.Value;
			_appUserOptions = appUserOptions.Value;
			_userManager = userManager;
            _env = env;
		}

        [Obsolete]
        public async Task CreateDefaultAdminUser()
		{
			try
			{
				var adminUser = new ApplicationUser
				{
					Email = _adminUserOptions.Email,
					UserName = _adminUserOptions.Username,
					EmailConfirmed = true,
                    ProfilePic = await GetDefaultProfilePic(),
                    PhoneNumber = "1234567890",
					PhoneNumberConfirmed = true,
					Firstname = _adminUserOptions.Firstname,
					Lastname = _adminUserOptions.Lastname,
					UserRole = "Administrator",
					IsActive = true,
					UserAddresses = new List<AddressModel>
					{
						new AddressModel {Country = _adminUserOptions.Country, Type = "Billing"},
						new AddressModel {Country = _adminUserOptions.Country, Type = "Shipping"}
					}
				};

				var result = await _userManager.CreateAsync(adminUser, _adminUserOptions.Password);

				if (result.Succeeded)
				{
					await _userManager.AddToRoleAsync(adminUser, "Administrator");
					Log.Information("Admin User Created {UserName}", adminUser.UserName);
				}
				else
				{
					var errorString = string.Join(",", result.Errors);
					Log.Error("Error while creating user {Error}", errorString);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error While creating user : {Error} * {StackTrace} * {InnerException} * {Source}",
					ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
			}
		}

        [Obsolete]
        public async Task CreateDefaultUser()
		{
            try
            {
                var appUser = new ApplicationUser
                {
                    Email = _appUserOptions.Email,
                    UserName = _appUserOptions.Username,
                    EmailConfirmed = true,
                    ProfilePic = await GetDefaultProfilePic(),
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    Firstname = _appUserOptions.Firstname,
                    Lastname = _appUserOptions.Lastname,
                    UserRole = "Customer",
                    IsActive = true,
                    UserAddresses = new List<AddressModel>
                    {
                        new AddressModel {Country = _appUserOptions.Country, Type = "Billing"},
                        new AddressModel {Country = _appUserOptions.Country, Type = "Shipping"}
                    }
                };

                var result = await _userManager.CreateAsync(appUser, _appUserOptions.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(appUser, "Customer");
                    Log.Information("App User Created {UserName}", appUser.UserName);
                }
                else
                {
                    var errorString = string.Join(",", result.Errors);
                    Log.Error("Error while creating user {Error}", errorString);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error While creating user : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

        }


        [Obsolete]
        private async Task<string> GetDefaultProfilePic()
        {
			try
			{
                // Create the Profile Image Path
                var profPicPath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}";
				var defaultPicPath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}default{Path.DirectorySeparatorChar}profile.jpeg";
				var extension = Path.GetExtension(defaultPicPath);
				var filename = DateTime.Now.ToString("yymmssfff");
				var path = Path.Combine(profPicPath, filename) + extension;
				var dbImagePath = Path.Combine($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}", filename) + extension;

				await using (Stream source = new FileStream(defaultPicPath, FileMode.Open))
				{
					await using Stream destination = new FileStream(path, FileMode.Create);
					await source.CopyToAsync(destination);
				}

				return dbImagePath;
			}
			catch (Exception ex)
			{
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
				    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return string.Empty;
        }

        public async Task SendEmailByGmailAsync(
            string? fromEmail, string? fromFullName, string subject, string messageBody, string toEmail, string toFullName, string? smtpUsername, string? smtpPassword, string? smtpHost, int smtpPort, bool smtpSSl)
        {
            try
            {
                var body = messageBody;
                var message = new MailMessage();
                message.To.Add(new MailAddress(toEmail, toFullName));
                message.From = new MailAddress(fromEmail, fromFullName);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var smtp = new SmtpClient();
                var credential = new NetworkCredential
                {
                    UserName = smtpUsername,
                    Password = smtpPassword
                };
                smtp.Credentials = credential;
                smtp.Host = smtpHost;
                smtp.Port = smtpPort;
                smtp.EnableSsl = smtpSSl;
                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }

        public async Task SendEmailBySendGridAsync(
            string? sendGridKey, string? fromEmail, string? fromFullName, string subject, string message, string email)
        {
            try
            {
               await Execute(sendGridKey, fromEmail, fromFullName, subject, message, email);
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }

        static async Task<Response> Execute(string apiKey, string fromEmail, string fromFullName, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromFullName);
            var to = new EmailAddress(email);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response;
        }
    }
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8604 // Possible null reference argument.
}


using AttributeService;
using CookieService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ModelService;
using Serilog;
using UserService;
using WritableOptionsService;

namespace Angular_with_Dotnet.Areas.Admin.Controllers
{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class EmailSettingsController : Controller
    {

        private readonly IUserSvc _userSvc;
        private readonly ICookieSvc _cookieSvc;
        private readonly IServiceProvider _provider;
        private readonly AppSettings _appSettings;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly IWritableSvc<SmtpOptions> _writableSmtpOptions;
        private readonly IWritableSvc<SendGridOptions> _writableSendGridOptions;
        private readonly IWritableSvc<SiteWideSettings> _writableSiteWideSettings;
        private AdminBaseViewModel adminBaseViewModel;

        public EmailSettingsController(
            IUserSvc userSvc,
            ICookieSvc cookieSvc,
            IServiceProvider provider,
            IOptions<AppSettings> appSettings,
            IOptions<DataProtectionKeys> dataProtectionkeys,
            IWritableSvc<SmtpOptions> writableSmtpOptions,
            IWritableSvc<SendGridOptions> writableSendGridOptions,
            IWritableSvc<SiteWideSettings> writableSiteWideSettings
        )
        {
            _userSvc = userSvc;
            _provider = provider;
            _cookieSvc = cookieSvc;
            _appSettings = appSettings.Value;
            _writableSmtpOptions = writableSmtpOptions;
            _dataProtectionKeys = dataProtectionkeys.Value;
            _writableSendGridOptions = writableSendGridOptions;
            _writableSiteWideSettings = writableSiteWideSettings;
        }

        public async Task<IActionResult> Index()
        {
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
            var protector = protectorProvider.CreateProtector(purpose: _dataProtectionKeys.ApplicationUserKey);
            var userProfile = await _userSvc.GetUserProfileByIDAsync(protector.Unprotect(_cookieSvc.Get("user_id")));
            var addUserModel = new AddUserModel();
            var protectorSendGrid = protectorProvider.CreateProtector(_dataProtectionKeys.SendGridProtectionKey);

            adminBaseViewModel = new AdminBaseViewModel
            {
                Profile = userProfile,
                AddUser = addUserModel,
                AppSettings = _appSettings,
                SmtpOption = _writableSmtpOptions.Value,
                SendGridOptions = _writableSendGridOptions.Value,
                SiteWideSettings = _writableSiteWideSettings.Value
            };
            adminBaseViewModel.SendGridOptions.SendGridKey = protectorSendGrid.Protect(adminBaseViewModel.SendGridOptions.SendGridKey);
            adminBaseViewModel.SmtpOption.SmtpPassword = protectorSendGrid.Protect(adminBaseViewModel.SmtpOption.SmtpPassword);

            return View("Index", adminBaseViewModel);

        }

        [AjaxOnly]
        [HttpPost]
        public IActionResult UpdateSendgridOptions([FromBody] SendGridOptions options)
        {
            try
            {
                // Checking to see ID key was not updated
                // We do not want the encrypted key from input to be updated
                var protectorProvider = _provider.GetService<IDataProtectionProvider>();
                var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                var decryptedkey = protector.Unprotect(options.SendGridKey);
                options.SendGridKey = decryptedkey;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred decrypting sndgrid key : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            var resultError = _writableSendGridOptions.Update((opt) =>
            {
                opt.FromEmail = options.FromEmail;
                opt.FromFullName = options.FromFullName;
                opt.SendGridKey = options.SendGridKey;
                opt.SendGridUser = options.SendGridUser;
                opt.IsDefault = options.IsDefault;
            });

            if (!resultError)
            {
                if (options.IsDefault) _writableSmtpOptions.Update((optSmtp) => { optSmtp.IsDefault = false; });
                return Ok(new { success = true });
            }

            return BadRequest(new { success = false });
        }

        [AjaxOnly]
        [HttpPost]
        public IActionResult UpdateSmtpOptions([FromBody] SmtpOptions options)
        {
            var resultError = _writableSmtpOptions.Update((sp) =>
            {
                sp.FromEmail = options.FromEmail;
                sp.FromFullName = options.FromFullName;
                sp.SmtpPassword = options.SmtpPassword;
                sp.SmtpUsername = options.SmtpUsername;
                sp.IsDefault = options.IsDefault;
                sp.SmtpHost = options.SmtpHost;
                sp.SmtpPort = options.SmtpPort;
                sp.SmtpSSl = options.SmtpSSl;
            });

            if (!resultError)
            {
                if (options.IsDefault) _writableSendGridOptions.Update((optSend) => { optSend.IsDefault = false; });
                return Ok(new { success = true });
            }

            return BadRequest(new { success = false });
        }
    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}


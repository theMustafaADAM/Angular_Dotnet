using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CookieService;
using ModelService;
using UserService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using WritableOptionsService;

namespace Angular_with_Dotnet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class ProfileController : Controller
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
        private static AdminBaseViewModel? AdminBaseViewModel;
        private readonly IWritableSvc<SiteWideSettings> _writableSiteWideSettings; 
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly IServiceProvider _provider;
        private readonly AppSettings _appSettings;
        private readonly ICookieSvc _cookieSvc;
        private readonly IUserSvc _userSvc;

        public ProfileController(
            IUserSvc userSvc,
            ICookieSvc cookieSvc,
            IServiceProvider provider,
            IOptions<AppSettings> appSettings,
            IOptions<DataProtectionKeys> dataProtectionKeys,
            IWritableSvc<SiteWideSettings> writableSiteWideSettings
        )
        {
            _userSvc = userSvc;
            _provider = provider;
            _cookieSvc = cookieSvc;
            _appSettings = appSettings.Value;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _writableSiteWideSettings = writableSiteWideSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await SetAdminBaseViewModel();
            return View("Index", AdminBaseViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Security()
        {
            await SetAdminBaseViewModel();
            return View("Security", AdminBaseViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Activity()
        {
            await SetAdminBaseViewModel();
            return View("Activity", AdminBaseViewModel);
        }

        private async Task SetAdminBaseViewModel()
        {
            var protectorProvider   = _provider.GetService<IDataProtectionProvider>();
            var protector           = protectorProvider.CreateProtector(purpose: _dataProtectionKeys.ApplicationUserKey);
            var userProfile         = await _userSvc.GetUserProfileByIDAsync(protector.Unprotect(_cookieSvc.Get("user_id")));
            var resetPassword       = new ResetPasswordViewModel();

            AdminBaseViewModel = new AdminBaseViewModel
            {
                AddUser = null,
                Dashbord = null,
                AppSettings = null,
                Profile = userProfile,
                ResetPassword = resetPassword,
                SiteWideSettings = _writableSiteWideSettings.Value
            };
        }

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
    }
}


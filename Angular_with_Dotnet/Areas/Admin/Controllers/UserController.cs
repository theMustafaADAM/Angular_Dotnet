using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttributeService;
using CookieService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ModelService;
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
    public class UserController : Controller
    {
        private readonly IUserSvc _userSvc;
        private readonly ICookieSvc _cookieSvc;
        private readonly IServiceProvider _provider;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly AppSettings _appSettings;
        private readonly IWritableSvc<SiteWideSettings> _writableSiteWideSettings;
        private AdminBaseViewModel adminBaseViewModel;

        public UserController(
            IUserSvc userSvc,
            ICookieSvc cookieSvc,
            IServiceProvider provider,
            IOptions<AppSettings> appSettings,
            IOptions<DataProtectionKeys> dataProtectionkeys,
            IWritableSvc<SiteWideSettings> writableSiteWideSettings

        )
        {
            _userSvc = userSvc;
            _provider = provider;
            _cookieSvc = cookieSvc;
            _appSettings = appSettings.Value;
            _dataProtectionKeys = dataProtectionkeys.Value;
            _writableSiteWideSettings = writableSiteWideSettings;

        }

        public async Task<IActionResult> Index()
        {
            var protectorProvider = _provider.GetService<IDataProtectionProvider>();
            var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
            var userProfile = await _userSvc.GetUserProfileByIDAsync(protector.Unprotect(_cookieSvc.Get("user_id")));
            var addUserModel = new AddUserModel();
            adminBaseViewModel = new AdminBaseViewModel
            {
                Profile = userProfile,
                AddUser = addUserModel,
                AppSettings = _appSettings,
                SiteWideSettings = _writableSiteWideSettings.Value
            };
            return View("Index", adminBaseViewModel);

        }

        [AjaxOnly]
        public IActionResult GetUsers()
        {
            return PartialView("_GetUsersLayout");
        }
    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}


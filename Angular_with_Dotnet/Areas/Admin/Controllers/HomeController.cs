using UserService;
using ModelService;
using CookieService;
using DashboardService;
using WritableOptionsService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;

namespace Angular_with_Dotnet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private readonly IUserSvc _userSvc;
        private readonly ICookieSvc _cookieSvc;
        private readonly AppSettings _appSettings;
        private readonly IServiceProvider _provider;
        private readonly IDashboardSvc _dashboardSvc;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly IWritableSvc<SiteWideSettings> _writableSiteWideSettings;
        private AdminBaseViewModel adminBaseViewModel;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public HomeController(
            IUserSvc userSvc,
            ICookieSvc cookieSvc,
            IServiceProvider provider,
            IDashboardSvc dashboardSvc,
            IOptions<AppSettings> appSettings,
            IOptions<DataProtectionKeys> dataProtectionkeys,
            IWritableSvc<SiteWideSettings> writableSiteWideSettings
        )
        {
            _userSvc = userSvc;
            _provider = provider;
            _cookieSvc = cookieSvc;
            _dashboardSvc = dashboardSvc;
            _appSettings = appSettings.Value;
            _dataProtectionKeys = dataProtectionkeys.Value;
            _writableSiteWideSettings = writableSiteWideSettings;
        }

        public async Task<IActionResult> Index()
        {
            var protectorProvider   = _provider.GetService<IDataProtectionProvider>();
            var protector           = protectorProvider.CreateProtector(purpose: _dataProtectionKeys.ApplicationUserKey);
            var userProfile         = await _userSvc.GetUserProfileByIDAsync(protector.Unprotect(_cookieSvc.Get("user_id")));
            var dashboard           = await _dashboardSvc.GetDashboardData();
            var addUserModel        = new AddUserModel();

            adminBaseViewModel = new AdminBaseViewModel
            {
                Dashbord = dashboard,
                Profile = userProfile,
                AddUser = addUserModel,
                AppSettings = _appSettings,
                SiteWideSettings = _writableSiteWideSettings.Value
            };
            return View("Index", adminBaseViewModel);
        }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}


using System;
using Serilog;
using System.Linq;
using AuthService;
using DataService;
using ModelService;
using CookieService;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Angular_with_Dotnet.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private DataProtectionKeys _dataProtectionKeys;
        private readonly IServiceProvider _provider;
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _db;
        private readonly ICookieSvc _cookieSvc;
        private readonly IAuthSvc _authSvc;
        private const string AccessToken = "access_token";
        private const string UserID = "user_id";
        string[] cookiesToDelete = { "twoFactorToken", "memberId", "rememberDevice", "user_id", "access_token" };

        public AccountController(ApplicationDbContext db,
            IOptions<DataProtectionKeys> dataProtectionKeys,
            IOptions<AppSettings> appSettings,
            IServiceProvider provider,
            ICookieSvc cookieSvc,
            IAuthSvc authSvc)
        {
            _dataProtectionKeys = dataProtectionKeys.Value;
            _appSettings = appSettings.Value;
            _cookieSvc = cookieSvc;
            _provider = provider;
            _authSvc = authSvc;
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            await Task.Delay(0);
            ViewData["ReturnUrl"] = returnUrl;
            try
            {
                // Check if user already logged In
                if (!Request.Cookies.ContainsKey(AccessToken) || !Request.Cookies.ContainsKey(UserID))
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model, string? returnUrl = null)
        {
            // First : get the return URL , the Url that user trying to access initially
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
#pragma warning disable CS8604 // Possible null reference argument.

                    var jwtToken = await _authSvc.Auth(model);
                    const int expireTime = 60; // set value to 60, as dont want admin cookie to stay in browser for longer

                    _cookieSvc.SetCookie("access_token", value: jwtToken.Token, expireTime);
                    _cookieSvc.SetCookie("user_id", value: jwtToken.UserID, expireTime);
                    _cookieSvc.SetCookie("username", value: jwtToken.Username, expireTime);

                    Log.Information($"User {model.Email} logged In.");
                    return Ok("Success");

                }
                catch (Exception ex)
                {
                    Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                        ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                }
            }

            ModelState.AddModelError("", "Invalid Username/Password was entered !");

            Log.Error("Invalid Username/Password was entered !");

            return Unauthorized("Please Check the Login Credentials \n- Invalid Username/Password was entered !");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.

                var userID = _cookieSvc.Get("user_id");
                if (userID != null)
                {
                    var protectorProvider = _provider.GetService<IDataProtectionProvider>();

                    IDataProtector protector = protectorProvider.CreateProtector(purpose: _dataProtectionKeys.ApplicationUserKey);

                    var unprotectedToken = protector.Unprotect(userID);

                    var rt = _db.Tokens.FirstOrDefault(t => t.UserID == unprotectedToken);

                    // First remove the token
                    if (rt != null) _db.Tokens.Remove(rt);

                    await _db.SaveChangesAsync();

                    // Then: remove all cookies
                    _cookieSvc.DeleteAllCookies(cookiesToDelete);
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
            }
            catch (Exception ex)
            {
                _cookieSvc.DeleteAllCookies(cookiesToDelete);
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            Log.Information("User Logged out. ");
            return RedirectToLocal(null);
        }

        private IActionResult RedirectToLocal(string? returnURL)
        {
            // Preventing Open Redirect attack
            return Url.IsLocalUrl(returnURL)
                ? Redirect(returnURL) : RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}


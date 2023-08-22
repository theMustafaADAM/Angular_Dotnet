using System;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;

namespace CookieService
{
    public class CookieSvc : ICookieSvc
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable SYSLIB0014 // Type or member is obsolete

        private readonly CookieOptions _cookieOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieSvc(CookieOptions cookieOptions, IHttpContextAccessor httpContextAccessor)
        {
            _cookieOptions = cookieOptions;
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetCookie(string key, string value, int? expireTime, bool isSecure, bool isHttpOnly)
        {
            _cookieOptions.Expires = expireTime.HasValue ? DateTime.Now.AddMinutes(expireTime.Value) : DateTime.Now.AddMilliseconds(10);
            _cookieOptions.Secure = isSecure;
            _cookieOptions.HttpOnly = isHttpOnly;
            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, _cookieOptions);
        }

        public void SetCookie(string key, string value, int? expireTime)
        {
            if (expireTime.HasValue)
            {
                _cookieOptions.Secure = true;
                _cookieOptions.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            }
            else
            {
                _cookieOptions.Secure = true;
                _cookieOptions.Expires = DateTime.Now.AddMilliseconds(10);

            }
            _cookieOptions.HttpOnly = true;
            _cookieOptions.SameSite = SameSiteMode.Strict;
            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, _cookieOptions);
        }

        public void DeleteAllCookies(IEnumerable<string> cookiesToDelete)
        {
            foreach (var key in cookiesToDelete)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
            }
        }

        public void DeleteCookie(string key)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
        }

        public string Get(string key)
        {
            return _httpContextAccessor.HttpContext.Request.Cookies[key];
        }

        public string GetUserIP()
        {
            string userIP = "unknown";

            try
            {
                userIP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userIP;
        }

        public string GetUserCountry()
        {
            try
            {
                string userIP = GetUserIP();
                // it's free service only for education porpuse
                string info = new WebClient().DownloadString("http://ipinfo.io/" + userIP);
                var IPInfo = JsonConvert.DeserializeObject<IPInfo>(info);
                RegionInfo regionInfo = new RegionInfo(name: IPInfo.Country);
                IPInfo.Country = regionInfo.EnglishName;

                if(!string.IsNullOrEmpty(userIP))
                {
                    return IPInfo.Country;
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return "unkown";
        }

        public string GetUserOS()
        {
            string userOS = "unknown";

            try
            {
                userOS = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userOS;
        }


#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore SYSLIB0014 // Type or member is obsolete
    }
}


using System;
using Microsoft.AspNetCore.Http;

namespace CookieService
{
	public interface ICookieSvc
	{
        void SetCookie(string key, string value, int? expireTime, bool isSecure, bool isHttpOnly);
        void SetCookie(string key, string value, int? expireTime);
        void DeleteAllCookies(IEnumerable<string> cookiesToDelete);
        void DeleteCookie(string key);
        string Get(string key);
        string GetUserCountry();
        string GetUserIP();
        string GetUserOS();

    }
}


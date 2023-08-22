using System;
using ModelService;

namespace AuthService
{
	public interface IAuthSvc
	{
        Task<TokenResponseModel> Auth(LoginViewModel model);
        Task<TokenResponseModel> Auth(TokenRequestModel model);
        Task<TokenResponseModel> GenerateNewToken();
        Task<bool> LogoutUserAsync();
        void DeleteAllCookies(IEnumerable<string> cookiesToDelete);
        void DeleteCookie(string name);

    }
}


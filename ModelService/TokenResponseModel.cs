using System;
using System.Security.Claims;

namespace ModelService
{
	public class TokenResponseModel
	{

        public string? Token { get; set; }			// Jwt token
		public string? RefreshToken { get; set; }	// refresh token
		public string? Role { get; set; }			// user role
		public string? Username { get; set; }		// user name
		public string? UserID { get; set; }			// user id
        public DateTime Expiration { get; set; }	// expiry time
		public DateTime RefreshTokenExpiration { get; set; }	// expiry time
		public bool TwoFactorLoginOn { get; set; }	// if 2 factor is on
		public ClaimsPrincipal? Principal { get; set; }
		public ResponseStatusInfoModel? ResponseInfo { get; set; }
    }
}


using System;
namespace ModelService
{
	public class AppSettings
	{
		public string? Site { get; set; }
		public string? Audience { get; set; }
		public string? ExpireTime { get; set; }
        public string? RtExpireTime { get; set; }
		public string? Secret { get; set; }
		public string? ClientID { get; set; }
		public bool ValidateIssuerSigninKey { get; set; }
		public bool ValidateIssuer { get; set; }
		public bool ValidateAudience { get; set; }
		public bool AllowSiteWideTokenRefresh { get; set; }
    }
}


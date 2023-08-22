using System;
namespace ModelService
{
	public class AdminBaseViewModel
    {
        public ProfileModel? Profile { get; set; }
        public AddUserModel? AddUser { get; set; }
        public DashboardModel? Dashbord { get; set; }
        public AppSettings? AppSettings { get; set; }
        public SendGridOptions? SendGridOptions { get; set; }
        public SmtpOptions? SmtpOption { get; set; }
        public ResetPasswordViewModel? ResetPassword { get; set; }
        public SiteWideSettings? SiteWideSettings { get; set; }
    }
}


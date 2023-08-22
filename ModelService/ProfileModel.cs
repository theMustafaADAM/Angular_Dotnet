using System;
using System.ComponentModel.DataAnnotations;

namespace ModelService
{
	public class ProfileModel
	{
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public string? Birthday { get; set; }
        public string? Gender { get; set; }
        public string? DisplayName { get; set; }
        public string? Firstname { get; set; }
        public string? Middlename { get; set; }
        public string? Lastname { get; set; }
        public string? UserRole { get; set; }
        public string? UserID { get; set; }
        public string? ProfilePic { get; set; }
        public bool IsTwoFactorOn { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsTermsAccepted { get; set; }
        public bool IsAccountLocked { get; set; }
        public bool IsEmployee { get; set; }
        public ICollection<AddressModel>? UserAddresses { get; set; }
        public ICollection<ActivityModel>? Activities { get; set; }
    }
}


using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ModelService
{
    public class ApplicationUser : IdentityUser
    {

        public string? Notes { get; set; }
        public string? DisplayName { get; set; }
        public string? Firstname { get; set; }
        public string? Middlename { get; set; }
        public string? Lastname { get; set; }
        public string? Gender { get; set; }
        public string? ProfilePic { get; set; }
        public string? Birthday { get; set; }
        public string? UserRole { get; set; }
        public bool IsProfileComplete { get; set; }
        public bool Terms { get; set; }
        public bool IsEmployee { get; set; }
        public bool RememberMe { get; set; }
        public bool IsActive { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{MM/dd/yyyy HH:mm:ss}")]
        public DateTime AccountCreatedOn { get; set; }
        public ICollection<AddressModel>? UserAddresses { get; set; }
    }
}


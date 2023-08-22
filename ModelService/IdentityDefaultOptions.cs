using System;
namespace ModelService
{
	public class IdentityDefaultOptions
	{
        /*----------------------------------------------------------------------------------------------*/
        /*                                      Password Properties                                     */
        /*----------------------------------------------------------------------------------------------*/
        public int PasswordRequiredLength { get; set; }
        public int PasswordRequireUniqueChars { get; set; }
        public bool PasswordRequireDigit { get; set; }
        public bool PasswordRequireNonAlphanumeric { get; set; }
        public bool PasswordRequireUppercase { get; set; }
        public bool PasswordRequireLowercase { get; set; }

        /*----------------------------------------------------------------------------------------------*/
        /*                                      Lockout Properties                                      */
        /*----------------------------------------------------------------------------------------------*/
        public int LockoutDefaultLockoutTimeSpanInMinutes { get; set; }
        public int LockoutMaxFailedAccessAttempts { get; set; }
        public bool LockoutAllowedForNewUsers { get; set; }

        /*----------------------------------------------------------------------------------------------*/
        /*                                      Users Properties                                        */
        /*----------------------------------------------------------------------------------------------*/
        public bool UserRequireUniqueEmail { get; set; }
        public bool SignInRequireConfirmedEmail { get; set; }
        public string? AccessDeniedPath { get; set; }
    }
}


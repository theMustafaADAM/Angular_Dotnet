using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActivityService;
using CookieService;
using DataService;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelService;
using Serilog;

namespace BackendService
{
#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class AdminSvc : IAdminSvc
    {
        [Obsolete]
        private readonly IHostingEnvironment _env;
        private readonly IServiceProvider _provider;
        private readonly IActivitySvc _activitySvc;
        private readonly ICookieSvc _cookieSvc;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private IDataProtector _protector;

        [Obsolete]
        public AdminSvc(
            ICookieSvc cookieSvc,
            IHostingEnvironment env,
            ApplicationDbContext db,
            IActivitySvc activitySvc,
            IServiceProvider provider,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IOptions<DataProtectionKeys> dataProtectionKeys
        )
        {
            _dataProtectionKeys = dataProtectionKeys.Value;
            _activitySvc = activitySvc;
            _userManager = userManager;
            _cookieSvc = cookieSvc;
            _provider = provider;
            _env = env;
            _db = db;
        }

        public async Task<ProfileModel?> GetUserProfileByUsernameAsync(string username)
        {
            var userProfile = new ProfileModel();

            try
            {
                var loggedInUserId = await GetLoggedInUserId();

                if (loggedInUserId != null)
                {
                    var user = await _userManager.FindByNameAsync(username);

                    if (user == null) return null;

                    userProfile = new ProfileModel
                    {
                        UserID = user.Id,
                        Email = user.Email,
                        Username = user.UserName,
                        Phone = user.PhoneNumber,
                        Birthday = user.Birthday,
                        Gender = user.Gender,
                        DisplayName = user.DisplayName,
                        Firstname = user.Firstname,
                        Middlename = user.Middlename,
                        Lastname = user.Lastname,
                        IsEmailVerified = user.EmailConfirmed,
                        IsPhoneVerified = user.PhoneNumberConfirmed,
                        IsTermsAccepted = user.Terms,
                        IsTwoFactorOn = user.TwoFactorEnabled,
                        ProfilePic = user.ProfilePic,
                        UserRole = user.UserRole,
                        IsAccountLocked = user.LockoutEnabled,
                        IsEmployee = user.IsEmployee,
                        UserAddresses = new List<AddressModel>(await _db.addresses.Where(x => x.UserID == user.Id).Select(n =>
                            new AddressModel()
                            {
                                AddressID = n.AddressID,
                                Line1 = n.Line1,
                                Line2 = n.Line2,
                                Unit = n.Unit,
                                Country = n.Country,
                                State = n.State,
                                City = n.City,
                                PostalCode = n.PostalCode,
                                Type = n.Type,
                                UserID = n.UserID
                            }).ToListAsync()),
                        Activities = new List<ActivityModel>(_db.Activities.Where(x => x.UserID == user.Id)).OrderByDescending(o => o.Date).Take(20).ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while Get User Profile By Username :  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return userProfile;
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            List<UserModel> usersList = new List<UserModel>();

            try
            {
                usersList = new List<UserModel>();

                var loggedInUserId = await GetLoggedInUserId();

                if (loggedInUserId != null)
                {
                    var result = await _db.ApplicationUsers.Where(x => x.IsActive).ToListAsync();
                    if (result != null)
                    {
                        usersList.AddRange(result.Select(item => new UserModel
                        {
                            Email = item.Email,
                            Username = item.UserName,
                            Firstname = item.Firstname,
                            IsProfileComplete = item.IsProfileComplete,
                            Lastname = item.Lastname,
                            ProfilePic = item.ProfilePic,
                            IsActive = item.IsActive
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while Get All Users :  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

            }
            return usersList;
        }

        public async Task<Dictionary<string, List<string>>> AddUserAsync(ApplicationUser user, string password)
        {
            IdentityResult result = new IdentityResult();
            var errorList = new List<string>();

            try
            {
                var loggedInUserId = await GetLoggedInUserId();

                if (loggedInUserId != null)
                {
                    result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                    {
                        // Update the Address table USerId Column
                        return CreateResponse(result.Succeeded, errorList);
                    }

                    errorList.AddRange(result.Errors.Select(error => error.Description));
                }
                else
                {
                    errorList.Add("You are not Authorized");
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while Add User : {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return CreateResponse(result.Succeeded, errorList);
        }

        [Obsolete]
        public async Task<bool> UpdateProfileAsync(string userId, IFormCollection formData)
        {
            ActivityModel activityModel = new ActivityModel();

            try
            {
                var loggedInUserId = await GetLoggedInUserId();

                if (loggedInUserId != null)
                {
                    var user = await _userManager.FindByIdAsync(userId);

                    if (user == null) return false;

                    await UpdateProfilePicAsync(formData, user);
                    user.UserRole = formData["UserRole"];
                    user.Email = formData["Email"];
                    user.Firstname = formData["FirstName"];
                    user.Birthday = formData["Birthday"];
                    user.Lastname = formData["LastName"];
                    user.Middlename = formData["MiddleName"];
                    user.DisplayName = formData["DisplayName"];
                    user.PhoneNumber = formData["Phone"];
                    user.Gender = formData["Gender"];
                    user.TwoFactorEnabled = Convert.ToBoolean(Convert.ToInt32(formData["IsTwoFactorOn"]));
                    user.EmailConfirmed = Convert.ToBoolean(Convert.ToInt32(formData["IsEmailVerified"]));
                    user.PhoneNumberConfirmed = Convert.ToBoolean(Convert.ToInt32(formData["IsPhoneVerified"]));
                    user.IsEmployee = Convert.ToBoolean(Convert.ToInt32(formData["IsEmployee"]));
                    user.LockoutEnabled = Convert.ToBoolean(Convert.ToInt32(formData["IsAccountLocked"]));

                    /* If Addresses exist we update them => If Addresses do not exist we add them */
                    await InsertOrUpdateAddress(user.Id, "Shipping", line1: formData["ShippingAddress.Line1"], line2: formData["ShippingAddress.Line2"], formData["ShippingAddress.Country"], state: formData["ShippingAddress.State"], city: formData["ShippingAddress.City"], formData[key: "ShippingAddress.PostalCode"], formData[key: "ShippingAddress.Unit"]);
                    await InsertOrUpdateAddress(user.Id, "Billing", line1: formData["BillingAddress.Line1"], formData[key: "BillingAddress.Line2"], formData["BillingAddress.Country"], formData[key: "BillingAddress.State"], city: formData["BillingAddress.City"], formData[key: "BillingAddress.PostalCode"], formData[key: "BillingAddress.Unit"]);

                    await _userManager.UpdateAsync(user);
                    await _userManager.AddToRoleAsync(user, user.UserRole);

                    activityModel.Date = DateTime.UtcNow;
                    activityModel.IPAddress = _cookieSvc.GetUserIP();
                    activityModel.Location = _cookieSvc.GetUserCountry();
                    activityModel.OperatingSystem = _cookieSvc.GetUserOS();
                    activityModel.Type = "Profile update successful";
                    activityModel.Icon = "fas fa-thumbs-up";
                    activityModel.Color = "success";
                    await _activitySvc.AddUserActivity(activityModel);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while Update Profile : {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return false;
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            try
            {
                var loggedInUserId = await GetLoggedInUserId();

                if (loggedInUserId != null)
                {
                    var user = await _userManager.FindByNameAsync(username);

                    if (user != null)
                    {
                        await using var dbContextTransaction = await _db.Database.BeginTransactionAsync();
                        try
                        {
                            user.IsActive = false;
                            _db.Entry(user).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                            await dbContextTransaction.CommitAsync();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("An error occurred while Delete User : {Error} {StackTrace} {InnerException} {Source}",
                                ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

                            await dbContextTransaction.RollbackAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while Delete User :  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return false;
        }

        public async Task<List<string>?> ResetPasswordAsync(string username)
        {
            var userDetails = new List<string>();
            ActivityModel activityModel = new ActivityModel();

            activityModel.Date = DateTime.UtcNow;
            activityModel.IPAddress = _cookieSvc.GetUserIP();
            activityModel.Location = _cookieSvc.GetUserCountry();
            activityModel.OperatingSystem = _cookieSvc.GetUserOS();


            try
            {
                var loggedInUserId = await GetLoggedInUserId();

                if (loggedInUserId != null)
                {
                    var user = await _userManager.FindByNameAsync(username);

                    if (user == null)
                    {
                        // Don't reveal that the user does not exist
                        return null;
                    }
                    // If user found - generate a token code
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var pass = await GeneratePassword(10);
                    var result = await _userManager.ResetPasswordAsync(user, code, pass);
                    activityModel.UserID = user.Id;


                    if (!result.Succeeded)
                    {
                        activityModel.Type = "Reset-Password failed";
                        activityModel.Icon = "far fa-times-circle";
                        activityModel.Color = "danger";
                        await _activitySvc.AddUserActivity(activityModel);
                        return null;
                    }

                    userDetails.Add(user.Email);
                    userDetails.Add(pass);
                }

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while Reset Password : {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            activityModel.Type = "Reset-Password successful";
            activityModel.Icon = "fas fa-thumbs-up";
            activityModel.Color = "success";
            await _activitySvc.AddUserActivity(activityModel);
            return userDetails;
        }

        private Task<string> GeneratePassword(int length)
        {
            const string alphabets = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string punctuation = "#?!@$%^&*-]";
            var password = "";
            var character = "";
            var rnd = new Random();

            while (password.Length < length)
            {
                try
                {
                    var randArray1 = Math.Ceiling(alphabets.Length * (decimal)rnd.Next()).ToString(CultureInfo.InvariantCulture).ToCharArray();
                    var randArray2 = Math.Ceiling(numbers.Length * (decimal)rnd.Next()).ToString(CultureInfo.InvariantCulture).ToCharArray();
                    var randArray3 = Math.Ceiling(punctuation.Length * (decimal)rnd.Next()).ToString(CultureInfo.InvariantCulture).ToCharArray();
                    var hold = alphabets[(int)char.GetNumericValue(randArray1[0])];
                    hold = (password.Length % 2 == 0) ? (char.ToUpper(hold)) : (hold);
                    character += hold;
                    character += numbers[(int)char.GetNumericValue(randArray2[0])];
                    character += punctuation[(int)char.GetNumericValue(randArray3[0])];
                    password = character;
                }
                catch (Exception ex)
                {
                    Log.Error("An error occurred while Generate Password : {Error} {StackTrace} {InnerException} {Source}",
                        ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                }

            }
            return Task.FromResult(password.Substring(0, length));
        }

        private async Task<string?> GetLoggedInUserId()
        {
            try
            {
                var protectorProvider = _provider.GetService<IDataProtectionProvider>();
                var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);
                var unprotectUserId = protector.Unprotect(_cookieSvc.Get("user_id"));

                var user = await _userManager.FindByIdAsync(unprotectUserId);

                if (user != null && user.UserRole == "Administrator")
                {
                    return unprotectUserId;
                }

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while decrypting user Id  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }

            return null;

        }

        private static Dictionary<string, List<string>> CreateResponse(bool response, List<string> errorList)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (response)
            {
                result.Add("Success", new List<string>());
                return result;
            }

            result.Add("Failed", errorList);
            return result;
        }

        [Obsolete]
        private async Task<ApplicationUser> UpdateProfilePicAsync(IFormCollection formData, ApplicationUser user)
        {
            // First we create an empty array to store old file info
            var oldProfilePic = new string[1];
            // we will store path of old file to delete in an empty array.
            oldProfilePic[0] = Path.Combine(_env.WebRootPath + user.ProfilePic);

            // Create the Profile Image Path
            var profPicPath = _env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}";

            // If we have received any files for update, then we update the file path after saving to server
            // else we return the user without any changes
            if (formData.Files.Count <= 0) return user;

            var extension = Path.GetExtension(formData.Files[0].FileName);
            var filename = DateTime.Now.ToString("yymmssfff");
            var path = Path.Combine(profPicPath, filename) + extension;
            var dbImagePath = Path.Combine($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}user{Path.DirectorySeparatorChar}profile{Path.DirectorySeparatorChar}", filename) + extension;

            user.ProfilePic = dbImagePath;

            // Copying New Files to the Server - profile Folder
            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await formData.Files[0].CopyToAsync(stream);
            }

            // Delete old file after successful update
            if (!System.IO.File.Exists(oldProfilePic[0])) return user;

            System.IO.File.SetAttributes(oldProfilePic[0], FileAttributes.Normal);
            System.IO.File.Delete(oldProfilePic[0]);

            return user;
        }

        private async Task InsertOrUpdateAddress(string userId, string type, string line1, string line2, string country,
            string state, string city, string postalcode, string unit)
        {
            var updateAddress = _db.addresses.FirstOrDefault(predicate: ad => ad.User.Id == userId && ad.Type == type);
            await using var dbContextTransaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var newAddress = new AddressModel();
                if (updateAddress != null)
                {
                    updateAddress.Line1 = line1;
                    updateAddress.Line2 = line2;
                    updateAddress.Country = country;
                    updateAddress.City = city;
                    updateAddress.State = state;
                    updateAddress.PostalCode = postalcode;
                    updateAddress.Unit = unit;
                    _db.Entry(updateAddress).State = EntityState.Modified;
                }
                else
                {
                    newAddress.Line1 = line1;
                    newAddress.Line2 = line2;
                    newAddress.Country = country;
                    newAddress.City = city;
                    newAddress.State = state;
                    newAddress.PostalCode = postalcode;
                    newAddress.Unit = unit;
                    _db.Entry(newAddress).State = EntityState.Added;
                }

                await _db.SaveChangesAsync();

                await dbContextTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbContextTransaction.RollbackAsync();

                Log.Error("An error occurred while Inserting Or Updateing Address : {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }
    }

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
}


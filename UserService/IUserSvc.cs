using System;
using Microsoft.AspNetCore.Http;
using ModelService;

namespace UserService
{
	public interface IUserSvc
	{
        Task<ProfileModel?> GetUserProfileByUsernameAsync(string username);
        Task<ProfileModel?> GetUserProfileByEmailAsync(string email);
        Task<ProfileModel?> GetUserProfileByIDAsync(string userID);
        Task<bool> CheckPasswordAsync(ProfileModel model, string password);
        Task<bool> ChangePasswordAsync(ProfileModel user, string newpassword);
        Task<bool> UpdateProfileAsync(IFormCollection formData);
        Task<bool> AddUserActivity(ActivityModel activityModel);
        Task<List<ActivityModel>> GetUserActivity(string username);
        Task<ResponseObject> RegisterUserAsync(RegisterViewModel model);
        Task<TwoFactorCodeModel>GenerateTwoFactorCodeAsync(bool v, string userId);
        Task<TwoFactorResponseModel> SendTwoFactorAsync(TwoFactorRequestModel model);
        Task<ResponseObject> ExpireUserSessionAsync(string userId);
        Task<ResponseObject> ForgotPassword(string email);
        Task<TwoFactorResponseModel> ValidateTwoFactorCodeAsync(string code);
        Task<ResponseObject>ResetPassword(ResetPasswordViewModel model);
    }
}


using System;
using Microsoft.AspNetCore.Http;
using ModelService;

namespace BackendService
{
	public interface IAdminSvc
	{
        Task<Dictionary<string, List<string>>> AddUserAsync(ApplicationUser user, string password);
        Task<bool> UpdateProfileAsync(string userId, IFormCollection formData);
        Task<ProfileModel?> GetUserProfileByUsernameAsync(string username);
        Task<List<string>> ResetPasswordAsync(string username);
        Task<bool> DeleteUserAsync(string username);
        Task<List<UserModel>> GetAllUsersAsync();
    }
}


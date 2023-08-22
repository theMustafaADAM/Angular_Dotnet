using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ModelService;

namespace RolesService
{
	public interface IRoleSvc
	{
        Task<IEnumerable<IdentityRole>> GetAllRolesAsync();
        Task<IEnumerable<RolePermission>> GetAllRolePermissionsAsync();
        Task<IEnumerable<PermissionType>> GetAllRolePermissionsTypeAsync();
        Task<bool> AddToRolesAsync(IFormCollection formData);
        Task<bool> DeleteRoleAsync(string roleID);
        Task<bool> UpdateRoleAsync(IFormCollection formData);
        Task<bool> AddRolePermissionAsync(string rolePermissionName);
        Task<bool> DeleteRolePermissionAsync(int rolePermissionID);


    }
}


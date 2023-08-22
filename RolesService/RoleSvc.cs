using DataService;
using ModelService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace RolesService
{
    public class RoleSvc : IRoleSvc
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.

        [Obsolete]
        private readonly IHostingEnvironment _env;
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        [Obsolete]
        public RoleSvc(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IHostingEnvironment env,
            ApplicationDbContext db
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
            _db = db;
        }

        public async Task<IEnumerable<IdentityRole>?> GetAllRolesAsync()
        {
            try
            {
                var result = await _db.ApplicationRoles.Include(x => x.Permissions).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
            return null;
        }

        public async Task<IEnumerable<RolePermission>> GetAllRolePermissionsAsync()
        {
            var result = await _db.PermissionsType.ToListAsync();
            return (IEnumerable<RolePermission>)result;
        }

        public async Task<IEnumerable<PermissionType>> GetAllRolePermissionsTypeAsync()
        {
            var result = await _db.RolePermissions.ToListAsync();
            return (IEnumerable<PermissionType>)result;
        }

        [Obsolete]
        public async Task<bool> AddToRolesAsync(IFormCollection formData)
        {
            var resultError = false;

            try
            {
                ApplicationRole applicationRole = JsonConvert.DeserializeObject<ApplicationRole>(formData["ApplicationRole"]);
                foreach (var permission in applicationRole.Permissions)
                {
                    permission.ApplicationRoleID = applicationRole.Id;
                }

                string iconPath = "";

                if (formData.Files.Count > 0)
                {
                    var extension = Path.GetExtension(formData.Files[0].FileName);
                    var filename = DateTime.Now.ToString("yymmssfff");
                    iconPath = Path.Combine(_env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}" +
                        $"roles{Path.DirectorySeparatorChar}icons{Path.DirectorySeparatorChar}", filename) + extension;
                    await using var stream = new FileStream(iconPath, FileMode.Create);
                    await formData.Files[0].CopyToAsync(stream);
                    iconPath = Path.Combine($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}" +
                        $"roles{Path.DirectorySeparatorChar}icons{Path.DirectorySeparatorChar}", filename) + extension;
                }
                else
                {
                    iconPath = $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}" +
                        $"roles{Path.DirectorySeparatorChar}icons{Path.DirectorySeparatorChar}default{Path.DirectorySeparatorChar}role.png";
                }

                applicationRole.RoleIcon = iconPath;
                applicationRole.Name = applicationRole.RoleName;
                applicationRole.NormalizedName = applicationRole.RoleName.ToUpper();

                await _db.ApplicationRoles.AddAsync(applicationRole);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                resultError = true;
            }

            return resultError;
        }

        public async Task<bool> DeleteRoleAsync(string roleID)
        {
            var resultError = false;

            try
            {
                var roleToDelete = await _roleManager.FindByIdAsync(roleID);

                if (roleToDelete != null)
                {
                    var rolePermissions = await _db.RolePermissions.Where(x => x.ApplicationRoleID == roleID).ToListAsync();

                    if (rolePermissions != null) _db.RolePermissions.RemoveRange(rolePermissions);
                    else resultError = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                resultError = true;
            }

            return resultError;
        }

        [Obsolete]
        public async Task<bool> UpdateRoleAsync(IFormCollection formData)
        {
            var resultError = false;
            var newPermissions = new List<RolePermission>();

            try
            {
                ApplicationRole applicationRole = JsonConvert.DeserializeObject<ApplicationRole>(formData["ApplicationRole"]);

                string iconPath = "";

                var roleToUpdate = _db.ApplicationRoles.Include(x => x.Permissions).FirstOrDefault(p => p.Id == applicationRole.Id);

                if (roleToUpdate != null)
                {
                    roleToUpdate.Name = applicationRole.RoleName;
                    roleToUpdate.NormalizedName = applicationRole.RoleName.ToUpper();

                    // Check if new role type exist
                    var permissionIDList = applicationRole.Permissions.Select(x => x.ID).ToList();

                    foreach (var pid in permissionIDList)
                    {
                        if (roleToUpdate.Permissions.All(x => x.ID != pid))
                        {
                            var entityToAdd = applicationRole.Permissions.FirstOrDefault(x => x.ID == pid);
                            newPermissions.Add(entityToAdd);
                        }
                    }

                    foreach ( var permission in newPermissions )
                    {
                        var result = applicationRole.Permissions.Any(x => x.ID == permission.ID);
                        if (result)
                        {
                            var rm = applicationRole.Permissions.FirstOrDefault(x => x.ID == permission.ID);
                            applicationRole.Permissions.Remove(rm);
                        }
                    }

                    roleToUpdate.IsActive = applicationRole.IsActive;
                    roleToUpdate.Handle = applicationRole.Handle;

                    if (formData.Files.Count > 0)
                    {
                        var extension = Path.GetExtension(formData.Files[0].FileName);
                        var filename = DateTime.Now.ToString("yymmssfff");
                        iconPath = Path.Combine(_env.WebRootPath + $"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}" +
                            $"roles{Path.DirectorySeparatorChar}icons{Path.DirectorySeparatorChar}", filename) + extension;
                        await using var stream = new FileStream(iconPath, FileMode.Create);
                        await formData.Files[0].CopyToAsync(stream);
                        iconPath = Path.Combine($"{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}" +
                            $"roles{Path.DirectorySeparatorChar}icons{Path.DirectorySeparatorChar}", filename) + extension;
                        roleToUpdate.RoleIcon = iconPath;
                    }

                    _db.Entry(roleToUpdate).State = EntityState.Modified;

                    foreach (var item in newPermissions )
                    {
                        item.ID = 0;
                        item.ApplicationRoleID = roleToUpdate.Id;
                    }

                    await _db.RolePermissions.AddRangeAsync(newPermissions);
                    await _db.SaveChangesAsync();                    
                }

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                resultError = true;
            }

            return resultError;
        }

        public async Task<bool> AddRolePermissionAsync(string rolePermissionName)
        {
            var resultError = false;

            try
            {
                var pt = new PermissionType { Type = rolePermissionName };

                await _db.PermissionsType.AddAsync(pt);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

                resultError = true;
            }

            return resultError;
        }

        public async Task<bool> DeleteRolePermissionAsync(int rolePermissionID)
        {
            var resultError = false;

            try
            {
                var roleTypeToDelete = await _db.RolePermissions.FindAsync(rolePermissionID);

                if (roleTypeToDelete != null) _db.RolePermissions.Remove(roleTypeToDelete);

                else resultError = true;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

                resultError = true;
            }

            return resultError;
        }

    }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
}


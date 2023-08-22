using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient.Server;
using RolesService;
using Serilog;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Angular_with_Dotnet.Controllers.Api.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = "Admin")]
    [AutoValidateAntiforgeryToken]
    public class UserRoleController : ControllerBase
    {
        private readonly IRoleSvc _roleSvc;

        public UserRoleController(IRoleSvc roleSvc)
        {
            _roleSvc = roleSvc;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetRoles()
        {
            var userRoles = await _roleSvc.GetAllRolesAsync();
            return Ok(userRoles);
        }

        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(IFormCollection formData)
        {
            try
            {
                var resultError = await _roleSvc.AddToRolesAsync(formData);

                if (!resultError)
                {
                    Log.Information("{Info}", "New Role Added.");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest();
            }
            return BadRequest();
        }

        [HttpPut("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(IFormCollection formData)
        {
            try
            {
                var resultError = await _roleSvc.UpdateRoleAsync(formData);

                if (!resultError) return Ok();
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

                return BadRequest();
            }
            return BadRequest();
        }

        [HttpDelete("[action]/{roleID}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole([FromRoute] string roleID)
        {
            try
            {
                var resultError = await _roleSvc.DeleteRoleAsync(roleID);

                if (!resultError)
                {
                    Log.Information("{Info}", $"Role With ID {roleID} Deleted. ");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

                return BadRequest();
            }
            return BadRequest();
        }

        [HttpPost("[action]/{rolePermissionName}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRolePermission([FromRoute] string rolePermissionName)
        {
            try
            {
                var resultError = await _roleSvc.AddRolePermissionAsync(rolePermissionName);

                if (!resultError)
                {
                    Log.Information("{Info}", $"Role type {rolePermissionName} Added.");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest();
            }
            return BadRequest();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllRolePermission()
        {
            try
            {
                var roleTypes = await _roleSvc.GetAllRolePermissionsAsync();
                return Ok(roleTypes);

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest();
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllRolePermissionType()
        {
            try
            {
                var roleTypes = await _roleSvc.GetAllRolePermissionsTypeAsync();
                return Ok(roleTypes);

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return BadRequest();
            }

        }

        [HttpDelete("[action]/{rolePermissionID}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoleType([FromRoute] string rolePermissionID)
        {
            try
            {
                var rolePermissionToDelete = Convert.ToInt32(rolePermissionID);
                var resultError = await _roleSvc.DeleteRolePermissionAsync(rolePermissionToDelete);

                if (!resultError)
                {
                    Log.Information("{Info}", $"Role Type With ID {rolePermissionToDelete} Deleted. ");
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);

                return BadRequest();
            }
            return BadRequest();

        }
    }
}


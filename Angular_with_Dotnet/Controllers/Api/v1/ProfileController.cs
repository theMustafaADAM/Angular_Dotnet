using CookieService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelService;
using UserService;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Angular_with_Dotnet.Controllers.Api.v1
{
#pragma warning disable CS8604 // Possible null reference argument.

    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = "User")]
    [AutoValidateAntiforgeryToken]
    public class ProfileController : ControllerBase
    {
        private readonly IUserSvc _userSvc;
        private readonly ICookieSvc _cookieSvc;

        public ProfileController(IUserSvc userSvc, ICookieSvc cookieSvc)
        {
            _cookieSvc = cookieSvc; _userSvc = userSvc;
        }

        [HttpGet("[action]/{username}")]
        public async Task<IActionResult> GetUserProfile([FromRoute] string username)
        {
            if (username == null) return BadRequest();

            var result = await _userSvc.GetUserProfileByUsernameAsync(username);

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("[action]/{email}")]
        public async Task<IActionResult> GetUserProfileByEmail([FromRoute] string email)
        {
            if (email == null) return BadRequest();

            var result = await _userSvc.GetUserProfileByEmailAsync(email);

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("[action]/{username}")]
        public async Task<IActionResult> GetUserActivity([FromRoute] string username)
        {
            var result = await _userSvc.GetUserActivity(username);

            if (result != null) return Ok(new { Message = "Fetched user activities successfully!", data = result });

            return BadRequest(new { Message = "Could not fetch user activities." });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> UpdateProfile(IFormCollection fromData)
        {
            ProfileModel model = new ProfileModel { Username = fromData["Username"] };

            var password = fromData["Password"].ToString();

            if (await _userSvc.CheckPasswordAsync(model, password))
            {
                var result = await _userSvc.UpdateProfileAsync(fromData);

                if (result)
                {
                    return Ok(new { Message = "Profile updated successfully!" });
                }
            }
            return BadRequest(new { Message = "Could not update profiel ." });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> ChangePassword([FromBody] ResetPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.OldPassword)) return BadRequest("Old Password must be supplied for password change.");

            if (!ModelState.IsValid) return BadRequest(model);

            ProfileModel? user = await _userSvc.GetUserProfileByEmailAsync(email: model.Email);

            // DO NOT reveal that the user is not exist :)
            if (user == null) return Ok(new { Message = "Passwrod changed successfully!" });

            if ( !await _userSvc.CheckPasswordAsync(user, model.OldPassword))
            {
                // Notify attempt was made , to change password was failed
                ActivityModel activityModel = new ActivityModel
                {
                    UserID = user.UserID,
                    Date = DateTime.UtcNow,
                    IPAddress = _cookieSvc.GetUserIP(),
                    Location = _cookieSvc.GetUserCountry(),
                    OperatingSystem = _cookieSvc.GetUserOS(),
                    Type = "Profile update failed - Invalid Old Password",
                    Icon = "fas fa-exclamation-triangle",
                    Color = "warning"
                };

                await _userSvc.AddUserActivity(activityModel);

                return BadRequest(new { Message = "Invalid Old Password." });
            }

            var result = await _userSvc.ChangePasswordAsync(user, model.Password);

            if (result) return Ok(new { Message = "Password changed successfully :) " });

            return BadRequest(new { Message = "Password could not changed. Try again later" });
        }

    }
#pragma warning restore CS8604 // Possible null reference argument.
}


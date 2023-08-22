 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModelService;
using UserService;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Angular_with_Dotnet.Controllers
{
    public class PsswordController : Controller
    {
        private readonly IUserSvc _userSvc;

        public PsswordController(IUserSvc userSvc)
        {
            _userSvc = userSvc;
        }

        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            return View(model);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdatePassword([FromBody] ResetPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = await _userSvc.ResetPassword(model);

                if (result.IsValid) return RedirectToAction("ResetPasswordConfirmation");
            }
            return BadRequest("Fail");
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}


using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ODTDOCXtoPDFConverter.Api.Models;

namespace ODTDOCXtoPDFConverter.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LogoutController : ControllerBase
    {

        private readonly SignInManager<ApplicationUser> _signInManager;
         
        public LogoutController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost("logout")]
        public async Task<ActionResult> LogoutUser()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}

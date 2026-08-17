using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ODTDOCXtoPDFConverter.Api.Models;
using ODTDOCXtoPDFConverter.Api.Services;

namespace ODTDOCXtoPDFConverter.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LoginController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult> AuthenticateLogin(LoginRequestModel loginCredentials, CancellationToken cancellationToken)
        {
            var result = await _signInManager.PasswordSignInAsync(loginCredentials.Username, loginCredentials.Password, isPersistent: false, lockoutOnFailure: true);

            if (!result.Succeeded)
                return Unauthorized();

            return Ok();
        }
     }
 }

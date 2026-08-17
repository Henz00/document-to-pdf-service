using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ODTDOCXtoPDFConverter.Api.Models;

namespace ODTDOCXtoPDFConverter.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class CredentialsCheckController : ControllerBase
    {
        [Authorize]
        [HttpGet("me")]
        public ActionResult CheckCredentials()
        {
            return Ok();
        }
    }
}

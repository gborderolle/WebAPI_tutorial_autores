using Microsoft.AspNetCore.Mvc;
using WebAPI_tutorial_recursos.DTOs;

namespace WebAPI_tutorial_recursos.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        [HttpPost("register")] //api/accounts/register
        public async Task<ActionResult<AuthenticationResponse>>
    }
}

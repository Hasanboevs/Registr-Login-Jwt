using _SharedClass.DTOs;
using _SharedClass.Interface;
using Microsoft.AspNetCore.Mvc;

namespace JwtApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IUserAccount userAccount) : ControllerBase
    {
        [HttpPost("register")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Register(UserDto user)
        {
            var res = await userAccount.CreateAccount(user);
            return Ok(res);
        }

        [HttpPost("login")]
        [ProducesResponseType(200)]

        public async Task<IActionResult> Login(LogInDto login)
        {
            var res = await userAccount.LoginAccount(login);
            return Ok(res);
        }

    }
}

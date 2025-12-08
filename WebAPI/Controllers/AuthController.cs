using Business.Abstract;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login(UserForLoginDto userForLoginDto)
        {
            var userToLogin = _authService.Login(userForLoginDto);
            if (!userToLogin.Success)
            {
                return BadRequest(new { Success = false, Message = userToLogin.Message });
            }

            var result = _authService.CreateAccessToken(userToLogin.Data);
            if (result.Success)
            {
                return Ok(result.Data);
            }

            return BadRequest(new { Success = false, Message = result.Message });
        }

        [HttpPost("register")]
        public IActionResult Register(UserForRegisterDto userDto)
        {
            var registerResult = _authService.Register(userDto);
            if (!registerResult.Success)
            {
                return BadRequest(new { Success = false, Message = registerResult.Message });
            }

            var result = _authService.CreateAccessToken(registerResult.Data);
            if (result.Success)
            {
                return Ok(result.Data);
            }

            return BadRequest(new { Success = false, Message = result.Message });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { Success = true, Message = "Çıkış başarılı" });
        }
    }
}

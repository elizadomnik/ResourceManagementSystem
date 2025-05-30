using Microsoft.AspNetCore.Mvc;
using ResourceManagementSystem.Core.DTOs.User;
using ResourceManagementSystem.Core.Interfaces;
using System.Threading.Tasks;

namespace ResourceManagementSystem.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Succeeded || result.Data == null)
            {
                return BadRequest(new { Errors = result.Errors });
            }

            return Ok(result.Data);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Succeeded || result.Data == null)
            {
                return Unauthorized(new { Errors = result.Errors }); // Używamy Unauthorized dla błędnego logowania
            }

            return Ok(result.Data);
        }
    }
}
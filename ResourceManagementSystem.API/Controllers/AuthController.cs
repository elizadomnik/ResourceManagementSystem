using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ResourceManagementSystem.Core.DTOs.User;
using ResourceManagementSystem.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
                return Unauthorized(new { Errors = result.Errors });
            }

            return Ok(result.Data);
        }
        
        [HttpPost("Logout")] 
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            
            await _authService.LogoutServiceSideAsync(userIdClaim.Value);
            
            return Ok(new { Message = "Logout request processed by server." });
        }

    }
}

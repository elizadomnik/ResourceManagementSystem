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
                return Unauthorized(new { Errors = result.Errors }); // Używamy Unauthorized dla błędnego logowania
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

// using System.Security.Claims;
// using Microsoft.AspNetCore.Mvc;
// using ResourceManagementSystem.Core.DTOs.User;
// using ResourceManagementSystem.Core.Interfaces;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.IdentityModel.JsonWebTokens;
// using ResourceManagementSystem.API.Data;
//
// namespace ResourceManagementSystem.API.Controllers
// {
//     [Route("[controller]")]
//     [ApiController]
//     public class AuthController : ControllerBase
//     {
//         private readonly IAuthService _authService;
//         private readonly ApplicationDbContext _context;
//
//         public AuthController(IAuthService authService, ApplicationDbContext context)
//         {
//             _authService = authService;
//             _context = context;
//         }
//
//         [HttpPost("Register")]
//         public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(ModelState);
//             }
//
//             var result = await _authService.RegisterAsync(registerDto);
//
//             if (!result.Succeeded || result.Data == null)
//             {
//                 return BadRequest(new { Errors = result.Errors });
//             }
//
//             return Ok(result.Data);
//         }
//
//         [HttpPost("Login")]
//         public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(ModelState);
//             }
//
//             var result = await _authService.LoginAsync(loginDto);
//
//             if (!result.Succeeded || result.Data == null)
//             {
//                 return Unauthorized(new { Errors = result.Errors });
//             }
//
//             return Ok(result.Data);
//         }
//         
//         [HttpPost("Logout")]
//         [Authorize] 
//         public async Task<IActionResult> Logout()
//         {
//             Response.Cookies.Delete("access_token", new CookieOptions
//             {
//                 HttpOnly = true,
//                 Secure = Request.IsHttps,
//                 SameSite = SameSiteMode.Lax,
//                 Path = "/" 
//             });
//
//             return Ok(new { Message = "Logged out successfully" });
//         }
//         
//         [HttpGet("Me")]
//         public async Task<IActionResult> GetCurrentUser()
//         {
//             Console.WriteLine("Requsest cookie" + Request.Cookies["access_token"]);
//             Console.WriteLine("Requsest path" + Request.Headers["Origin"].ToString());
//             
//             var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub);
//             if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
//             {
//                 return Unauthorized("User ID not found in token.");
//             }
//
//             var user = await _context.Users.FindAsync(userId);
//             if (user == null)
//             {
//                 return NotFound("User not found.");
//             }
//
//             var expClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
//             DateTime expiresAt = DateTime.UnixEpoch;
//
//             if (expClaim != null && long.TryParse(expClaim.Value, out long unixEpochSeconds))
//             {
//                 expiresAt = DateTime.UnixEpoch.AddSeconds(unixEpochSeconds).ToUniversalTime();
//             }
//
//             return Ok(new AuthResponseDto
//             {
//                 UserId = user.Id.ToString(),
//                 Username = user.Username,
//                 Email = user.Email,
//                 ExpiresAt = expiresAt // Dodajemy czas wygaśnięcia odczytany z tokenu
//             });
//         }
//     }
// }
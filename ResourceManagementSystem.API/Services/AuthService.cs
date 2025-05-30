using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ResourceManagementSystem.API.Data;
using ResourceManagementSystem.Core.DTOs.User;
using ResourceManagementSystem.Core.Entities;
using ResourceManagementSystem.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; 
using System.Linq;
using System.Security.Claims; 
using System.Text;
using System.Threading.Tasks;

namespace ResourceManagementSystem.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Succeeded, AuthResponseDto? Data, string[] Errors)> RegisterAsync(UserRegisterDto registerDto, string roleName = "User")
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return (false, null, new[] { "Username already exists." });
            }

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return (false, null, new[] { "Email already in use." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                return (false, null, new[] { $"Role '{roleName}' not found." });
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginResult = await LoginAsync(new UserLoginDto { Email = user.Email, Password = registerDto.Password });
            return loginResult;
        }

        public async Task<(bool Succeeded, AuthResponseDto? Data, string[] Errors)> LoginAsync(UserLoginDto loginDto)
        {
            var user = await _context.Users.Include(u => u.Role) // Załaduj rolę użytkownika
                                       .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return (false, null, new[] { "Invalid email or password." });
            }

            var tokenString = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiryHours"] ?? "1"));


            var authResponse = new AuthResponseDto
            {
                UserId = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Token = tokenString,
                ExpiresAt = expiresAt
            };

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, authResponse, Array.Empty<string>());
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var expiryHours = Convert.ToDouble(_configuration["Jwt:ExpiryHours"] ?? "1");


            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT Key, Issuer or Audience not configured properly in appsettings.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject (user ID)
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Username), // Można użyć też ClaimTypes.Name
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
            };

            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            }

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
using ResourceManagementSystem.Core.DTOs.User;
using System.Threading.Tasks;

namespace ResourceManagementSystem.Core.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Succeeded, AuthResponseDto? Data, string[] Errors)> RegisterAsync(UserRegisterDto registerDto, string roleName = "User");
        Task<(bool Succeeded, AuthResponseDto? Data, string[] Errors)> LoginAsync(UserLoginDto loginDto);
        Task<(bool Succeeded, string[] Errors)> LogoutServiceSideAsync(string userId); 
    }
}
using System.ComponentModel.DataAnnotations;

namespace ResourceManagementSystem.Core.DTOs.User
{
    public class UserLoginDto
    {
        [Required]
        [EmailAddress] // Lub Username, zależy od preferencji logowania
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
using System.ComponentModel.DataAnnotations;

namespace ResourceManagementSystem.Core.DTOs.User
{
    public class UserRegisterDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)] // Dodajmy walidację długości hasła
        public string Password { get; set; } = string.Empty;
    }
}
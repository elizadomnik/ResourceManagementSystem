using ResourceManagementSystem.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace ResourceManagementSystem.Core.DTOs.Resource
{
    public class UpdateResourceDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public ResourceType Type { get; set; } = ResourceType.Other;
    }
}
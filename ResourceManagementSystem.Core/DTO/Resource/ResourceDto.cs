using ResourceManagementSystem.Core.Entities; // Dla ResourceType
using System;

namespace ResourceManagementSystem.Core.DTOs.Resource
{
    public class ResourceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public ResourceType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string? CreatedByUsername { get; set; }
        public string? LastUpdatedByUsername { get; set; } 
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Dla ForeignKey

namespace ResourceManagementSystem.Core.Entities
{
    public enum ResourceType
    {
        Laptop,
        Peripheral,
        Cable,
        Document,
        Software,
        Hardware,
        Tool,
        Other
    }

    public class Resource
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public ResourceType Type { get; set; } = ResourceType.Other;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public virtual User? CreatedBy { get; set; }

        public Guid? LastUpdatedById { get; set; }
        [ForeignKey("LastUpdatedById")]
        public virtual User? LastUpdatedBy { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
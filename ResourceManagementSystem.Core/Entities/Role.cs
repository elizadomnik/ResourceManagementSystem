using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ResourceManagementSystem.Core.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required] 
        [StringLength(50)] 
        public string Name { get; set; } = string.Empty; 


        public virtual ICollection<User> Users { get; set; } = new List<User>();
        
        // public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
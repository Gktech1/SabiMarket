namespace SabiMarket.Domain.Entities.UserManagement
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;

    public class ApplicationRole : IdentityRole
    {
        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        // New properties to match UI requirements
        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        [MaxLength(100)]
        public string LastModifiedBy { get; set; }

        // Collection of permissions as shown in the UI
        public virtual ICollection<RolePermission> Permissions { get; set; }

        public ApplicationRole() : base()
        {
            Permissions = new HashSet<RolePermission>();
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            Permissions = new HashSet<RolePermission>();
        }
    }

    public class RolePermission
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public bool IsGranted { get; set; }

        public string RoleId { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }


    /* public class ApplicationRole : IdentityRole<string>
     {
         public string Description { get; set; }
         public bool IsActive { get; set; }
         public DateTime CreatedAt { get; set; }
     }*/

    /* public class ApplicationRole : IdentityRole 
     {
         public string Description { get; set; }
         public bool IsActive { get; set; }
         public DateTime CreatedAt { get; set; }


         public ApplicationRole() : base()
         {
         }

         public ApplicationRole(string roleName) : base(roleName)
         {
         }
     }*/


}

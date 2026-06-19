using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("Roles")]
    public class AppRole
    {
        public AppRole()
        {
            Users = new HashSet<AppUser>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Index("IX_Roles_Name", IsUnique = true)]
        public string Name { get; set; }

        public virtual ICollection<AppUser> Users { get; set; }
    }
}
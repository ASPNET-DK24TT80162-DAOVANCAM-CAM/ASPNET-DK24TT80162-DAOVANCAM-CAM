using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("Brands")]
    public class Brand
    {
        public Brand()
        {
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Index("IX_Brand_Name", IsUnique = true)]
        public string Name { get; set; }

        [Required]
        [StringLength(120)]
        [Index("IX_Brand_Slug", IsUnique = true)]
        public string Slug { get; set; }

        [StringLength(400)]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
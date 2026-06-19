using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("Sizes")]
    public class ShoeSize
    {
        public ShoeSize()
        {
            Variants = new HashSet<ProductVariant>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Index("IX_Size_Name", IsUnique = true)]
        public string Name { get; set; }

        public int SortOrder { get; set; }

        public virtual ICollection<ProductVariant> Variants { get; set; }
    }
}
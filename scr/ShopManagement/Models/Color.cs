using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("Colors")]
    public class Color
    {
        public Color()
        {
            Variants = new HashSet<ProductVariant>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Index("IX_Color_Name", IsUnique = true)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Code { get; set; }

        [StringLength(20)]
        public string HexValue { get; set; }

        public virtual ICollection<ProductVariant> Variants { get; set; }
    }
}
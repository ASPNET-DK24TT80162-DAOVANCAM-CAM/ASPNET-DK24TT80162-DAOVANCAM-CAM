using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(300)]
        public string ImageUrl { get; set; }

        [StringLength(200)]
        public string AltText { get; set; }

        public int SortOrder { get; set; }

        public bool IsPrimary { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
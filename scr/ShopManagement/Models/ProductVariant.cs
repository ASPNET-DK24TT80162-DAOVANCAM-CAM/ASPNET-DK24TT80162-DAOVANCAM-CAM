using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("ProductVariants")]
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int ColorId { get; set; }

        public int SizeId { get; set; }

        [Required]
        [StringLength(80)]
        [Index("IX_ProductVariant_Sku", IsUnique = true)]
        public string Sku { get; set; }

        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("ColorId")]
        public virtual Color Color { get; set; }

        [ForeignKey("SizeId")]
        public virtual ShoeSize Size { get; set; }
    }
}
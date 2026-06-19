using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("CartItems")]
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }

        public int ProductVariantId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "money")]
        public decimal UnitPrice { get; set; }

        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        [ForeignKey("ProductVariantId")]
        public virtual ProductVariant ProductVariant { get; set; }
    }
}
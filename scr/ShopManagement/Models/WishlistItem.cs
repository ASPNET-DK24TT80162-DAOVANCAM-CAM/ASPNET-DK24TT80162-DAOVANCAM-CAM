using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("WishlistItems")]
    public class WishlistItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
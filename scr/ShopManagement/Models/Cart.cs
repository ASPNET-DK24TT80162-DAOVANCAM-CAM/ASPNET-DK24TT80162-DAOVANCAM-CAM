using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("Carts")]
    public class Cart
    {
        public Cart()
        {
            Items = new HashSet<CartItem>();
        }

        public int Id { get; set; }

        public int UserId { get; set; }

        public bool IsCheckedOut { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        public virtual ICollection<CartItem> Items { get; set; }
    }
}
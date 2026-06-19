using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("Orders")]
    public class Order
    {
        public Order()
        {
            Items = new HashSet<OrderItem>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Index("IX_Order_Code", IsUnique = true)]
        public string OrderCode { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string ReceiverName { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(250)]
        public string ShippingAddress { get; set; }

        [StringLength(500)]
        public string Note { get; set; }

        [Column(TypeName = "money")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "money")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "money")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(30)]
        public string OrderStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; }
    }
}
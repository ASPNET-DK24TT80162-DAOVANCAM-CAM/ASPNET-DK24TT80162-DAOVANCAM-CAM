using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("Users")]
    public class AppUser
    {
        public AppUser()
        {
            Wishlists = new HashSet<WishlistItem>();
            Carts = new HashSet<Cart>();
            Orders = new HashSet<Order>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(150)]
        [Index("IX_Users_Email", IsUnique = true)]
        public string Email { get; set; }

        [Required]
        [StringLength(250)]
        public string Address { get; set; }

        [Required]
        [StringLength(20)]
        [Index("IX_Users_CitizenId", IsUnique = true)]
        public string CitizenId { get; set; }

        [Required]
        [StringLength(512)]
        public string PasswordHash { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual AppRole Role { get; set; }

        public virtual ICollection<WishlistItem> Wishlists { get; set; }

        public virtual ICollection<Cart> Carts { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagement.Models
{
    [Table("Products")]
    public class Product
    {
        public Product()
        {
            Images = new HashSet<ProductImage>();
            Variants = new HashSet<ProductVariant>();
            WishlistItems = new HashSet<WishlistItem>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(160)]
        public string Name { get; set; }

        [Required]
        [StringLength(180)]
        [Index("IX_Product_Slug", IsUnique = true)]
        public string Slug { get; set; }

        [Required]
        [StringLength(80)]
        [Index("IX_Product_Sku", IsUnique = true)]
        public string Sku { get; set; }

        [StringLength(400)]
        public string ShortDescription { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        public int BrandId { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [StringLength(20)]
        public string Gender { get; set; }

        [Column(TypeName = "money")]
        public decimal BasePrice { get; set; }

        [Column(TypeName = "money")]
        public decimal? SalePrice { get; set; }

        [StringLength(300)]
        public string ThumbnailImage { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; }

        public virtual ICollection<ProductVariant> Variants { get; set; }

        public virtual ICollection<WishlistItem> WishlistItems { get; set; }
    }
}
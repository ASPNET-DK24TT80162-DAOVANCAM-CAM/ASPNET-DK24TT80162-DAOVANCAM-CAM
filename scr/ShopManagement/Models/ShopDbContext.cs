using System.Data.Entity;

namespace ShopManagement.Models
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext()
            : base("ShopDbContext")
        {
        }

        public DbSet<AppRole> Roles { get; set; }

        public DbSet<AppUser> Users { get; set; }

        public DbSet<Brand> Brands { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Color> Colors { get; set; }

        public DbSet<ShoeSize> Sizes { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<ProductVariant> ProductVariants { get; set; }

        public DbSet<WishlistItem> WishlistItems { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using ShopManagement.Models;

namespace ShopManagement.Infrastructure
{
    public static class DbBootstrapper
    {
        public static void Initialize()
        {
            Database.SetInitializer<ShopDbContext>(null);
            EnsureDatabaseExists();

            using (var db = new ShopDbContext())
            {
                EnsureSchema(db);
                SeedRoles(db);
                SeedUsers(db);
                SeedCatalog(db);
            }
        }

        private static void EnsureDatabaseExists()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["ShopDbContext"].ConnectionString;
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            var escapedDatabaseName = databaseName.Replace("]", "]]" );
            builder.InitialCatalog = "master";

            using (var connection = new SqlConnection(builder.ConnectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format("IF DB_ID(N'{0}') IS NULL CREATE DATABASE [{1}]", databaseName.Replace("'", "''"), escapedDatabaseName);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureSchema(ShopDbContext db)
        {
            var createRolesTable = @"
IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL UNIQUE
    )
END";

            var createUsersTable = @"
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Email NVARCHAR(150) NOT NULL UNIQUE,
        Address NVARCHAR(250) NOT NULL,
        CitizenId NVARCHAR(20) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(512) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT(GETDATE()),
        RoleId INT NOT NULL,
        CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id)
    )
END";

            var createBrandsTable = @"
IF OBJECT_ID(N'dbo.Brands', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Brands
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Slug NVARCHAR(120) NOT NULL UNIQUE,
        Description NVARCHAR(400) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Brands_IsActive DEFAULT(1)
    )
END";

            var createCategoriesTable = @"
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        Slug NVARCHAR(120) NOT NULL UNIQUE,
        Description NVARCHAR(400) NULL,
        ParentCategoryId INT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT(1),
        CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES dbo.Categories(Id)
    )
END";

            var createColorsTable = @"
IF OBJECT_ID(N'dbo.Colors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Colors
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL UNIQUE,
        Code NVARCHAR(20) NULL,
        HexValue NVARCHAR(20) NULL
    )
END";

            var createSizesTable = @"
IF OBJECT_ID(N'dbo.Sizes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sizes
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(20) NOT NULL UNIQUE,
        SortOrder INT NOT NULL
    )
END";

            var createProductsTable = @"
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(160) NOT NULL,
        Slug NVARCHAR(180) NOT NULL UNIQUE,
        Sku NVARCHAR(80) NOT NULL UNIQUE,
        ShortDescription NVARCHAR(400) NULL,
        Description NVARCHAR(2000) NULL,
        BrandId INT NOT NULL,
        CategoryId INT NOT NULL,
        Gender NVARCHAR(20) NOT NULL,
        BasePrice MONEY NOT NULL,
        SalePrice MONEY NULL,
        ThumbnailImage NVARCHAR(300) NULL,
        IsFeatured BIT NOT NULL CONSTRAINT DF_Products_IsFeatured DEFAULT(0),
        IsActive BIT NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT(1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT(GETDATE()),
        CONSTRAINT FK_Products_Brands FOREIGN KEY (BrandId) REFERENCES dbo.Brands(Id),
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
    )
END";

            var createProductImagesTable = @"
IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductImages
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductId INT NOT NULL,
        ImageUrl NVARCHAR(300) NOT NULL,
        AltText NVARCHAR(200) NULL,
        SortOrder INT NOT NULL,
        IsPrimary BIT NOT NULL CONSTRAINT DF_ProductImages_IsPrimary DEFAULT(0),
        CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    )
END";

            var createProductVariantsTable = @"
IF OBJECT_ID(N'dbo.ProductVariants', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductVariants
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductId INT NOT NULL,
        ColorId INT NOT NULL,
        SizeId INT NOT NULL,
        Sku NVARCHAR(80) NOT NULL UNIQUE,
        Price MONEY NOT NULL,
        StockQuantity INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ProductVariants_IsActive DEFAULT(1),
        CONSTRAINT FK_ProductVariants_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
        CONSTRAINT FK_ProductVariants_Colors FOREIGN KEY (ColorId) REFERENCES dbo.Colors(Id),
        CONSTRAINT FK_ProductVariants_Sizes FOREIGN KEY (SizeId) REFERENCES dbo.Sizes(Id)
    )
END";

            var createWishlistTable = @"
IF OBJECT_ID(N'dbo.WishlistItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WishlistItems
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        ProductId INT NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_WishlistItems_CreatedAt DEFAULT(GETDATE()),
        CONSTRAINT FK_WishlistItems_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_WishlistItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    )
    CREATE UNIQUE INDEX UX_Wishlist_User_Product ON dbo.WishlistItems(UserId, ProductId)
END";

            var createCartsTable = @"
IF OBJECT_ID(N'dbo.Carts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Carts
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        IsCheckedOut BIT NOT NULL CONSTRAINT DF_Carts_IsCheckedOut DEFAULT(0),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Carts_CreatedAt DEFAULT(GETDATE()),
        UpdatedAt DATETIME NOT NULL CONSTRAINT DF_Carts_UpdatedAt DEFAULT(GETDATE()),
        CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    )
END";

            var createCartItemsTable = @"
IF OBJECT_ID(N'dbo.CartItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CartId INT NOT NULL,
        ProductVariantId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice MONEY NOT NULL,
        CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES dbo.Carts(Id),
        CONSTRAINT FK_CartItems_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES dbo.ProductVariants(Id)
    )
    CREATE UNIQUE INDEX UX_CartItems_Cart_Variant ON dbo.CartItems(CartId, ProductVariantId)
END";

            var createOrdersTable = @"
IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        OrderCode NVARCHAR(50) NOT NULL UNIQUE,
        UserId INT NOT NULL,
        ReceiverName NVARCHAR(150) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        ShippingAddress NVARCHAR(250) NOT NULL,
        Note NVARCHAR(500) NULL,
        Subtotal MONEY NOT NULL,
        ShippingFee MONEY NOT NULL,
        DiscountAmount MONEY NOT NULL,
        TotalAmount MONEY NOT NULL,
        PaymentMethod NVARCHAR(30) NOT NULL,
        OrderStatus NVARCHAR(30) NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT(GETDATE()),
        CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
    )
END";

            var createOrderItemsTable = @"
IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductVariantId INT NOT NULL,
        ProductName NVARCHAR(180) NOT NULL,
        ColorName NVARCHAR(50) NOT NULL,
        SizeName NVARCHAR(20) NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice MONEY NOT NULL,
        LineTotal MONEY NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id),
        CONSTRAINT FK_OrderItems_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES dbo.ProductVariants(Id)
    )
END";

            db.Database.ExecuteSqlCommand(createRolesTable);
            db.Database.ExecuteSqlCommand(createUsersTable);
            db.Database.ExecuteSqlCommand(createBrandsTable);
            db.Database.ExecuteSqlCommand(createCategoriesTable);
            db.Database.ExecuteSqlCommand(createColorsTable);
            db.Database.ExecuteSqlCommand(createSizesTable);
            db.Database.ExecuteSqlCommand(createProductsTable);
            db.Database.ExecuteSqlCommand(createProductImagesTable);
            db.Database.ExecuteSqlCommand(createProductVariantsTable);
            db.Database.ExecuteSqlCommand(createWishlistTable);
            db.Database.ExecuteSqlCommand(createCartsTable);
            db.Database.ExecuteSqlCommand(createCartItemsTable);
            db.Database.ExecuteSqlCommand(createOrdersTable);
            db.Database.ExecuteSqlCommand(createOrderItemsTable);
        }

        private static void SeedRoles(ShopDbContext db)
        {
            var roleNames = new[] { "Admin", "Manager", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!db.Roles.Any(role => role.Name == roleName))
                {
                    db.Roles.Add(new AppRole { Name = roleName });
                }
            }

            db.SaveChanges();
        }

        private static void SeedCatalog(ShopDbContext db)
        {
            EnsureBrands(db);
            EnsureCategories(db);
            EnsureColors(db);
            EnsureSizes(db);
            EnsureProducts(db);
        }

        private static void EnsureBrands(ShopDbContext db)
        {
            EnsureBrand(db, "StrideLab", "stridelab", "Athletic-inspired urban sneakers");
            EnsureBrand(db, "Veloria", "veloria", "Minimal premium leather shoes");
            EnsureBrand(db, "MonoStep", "monostep", "Unisex daily movement shoes");
            db.SaveChanges();
        }

        private static void EnsureCategories(ShopDbContext db)
        {
            EnsureCategory(db, "Sneakers", "sneakers", "Street-ready sneakers for every day");
            EnsureCategory(db, "Loafers", "loafers", "Smart casual loafers");
            EnsureCategory(db, "Running", "running", "Performance running shoes");
            EnsureCategory(db, "Boots", "boots", "Durable boots for style and weather");
            db.SaveChanges();
        }

        private static void EnsureColors(ShopDbContext db)
        {
            EnsureColor(db, "Black", "BLK", "#111111");
            EnsureColor(db, "White", "WHT", "#F8F9FA");
            EnsureColor(db, "Navy", "NVY", "#1A2C4A");
            EnsureColor(db, "Beige", "BEG", "#D8C3A5");
            EnsureColor(db, "Olive", "OLV", "#6C7A3B");
            db.SaveChanges();
        }

        private static void EnsureSizes(ShopDbContext db)
        {
            var sizeNames = new[] { "38", "39", "40", "41", "42", "43", "44" };
            for (var index = 0; index < sizeNames.Length; index++)
            {
                var sizeName = sizeNames[index];
                if (!db.Sizes.Any(item => item.Name == sizeName))
                {
                    db.Sizes.Add(new ShoeSize
                    {
                        Name = sizeName,
                        SortOrder = index + 1
                    });
                }
            }

            db.SaveChanges();
        }

        private static void EnsureProducts(ShopDbContext db)
        {
            if (db.Products.Any())
            {
                return;
            }

            AddProduct(db, "Aero Pulse Men Runner", "aero-pulse-men-runner", "STR-M-001", "Men", "running", "stridelab", 1450000m, 1250000m,
                "Lightweight men runner with breathable mesh and rebound sole.",
                "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=900&q=80",
                new[] { "Black", "White" });

            AddProduct(db, "Veloria City Loafer", "veloria-city-loafer", "VEL-M-002", "Men", "loafers", "veloria", 1680000m, null,
                "Clean silhouette loafer crafted for office-to-evening wear.",
                "https://images.unsplash.com/photo-1449824913935-59a10b8d2000?auto=format&fit=crop&w=900&q=80",
                new[] { "Black", "Beige" });

            AddProduct(db, "CloudStep Women Sneaker", "cloudstep-women-sneaker", "STR-W-003", "Women", "sneakers", "stridelab", 1390000m, 1190000m,
                "Soft-cushion women sneaker designed for urban comfort.",
                "https://images.unsplash.com/photo-1514989940723-e8e51635b782?auto=format&fit=crop&w=900&q=80",
                new[] { "White", "Navy" });

            AddProduct(db, "Veloria Chelsea Boot", "veloria-chelsea-boot", "VEL-W-004", "Women", "boots", "veloria", 1890000m, null,
                "Modern ankle boot with flexible side panels and supportive footbed.",
                "https://images.unsplash.com/photo-1520639888713-7851133b1ed0?auto=format&fit=crop&w=900&q=80",
                new[] { "Black", "Olive" });

            AddProduct(db, "MonoStep Daily Flow", "monostep-daily-flow", "MON-U-005", "Unisex", "sneakers", "monostep", 1290000m, 990000m,
                "Unisex sneaker with minimal styling and all-day support.",
                "https://images.unsplash.com/photo-1552346154-21d32810aba3?auto=format&fit=crop&w=900&q=80",
                new[] { "Black", "White", "Beige" });

            AddProduct(db, "Storm Sprint Trainer", "storm-sprint-trainer", "STR-U-006", "Unisex", "running", "stridelab", 1490000m, null,
                "High-grip trainer for quick runs and gym sessions.",
                "https://images.unsplash.com/photo-1525966222134-fcfa99b8ae77?auto=format&fit=crop&w=900&q=80",
                new[] { "Navy", "Olive" });
        }

        private static void AddProduct(
            ShopDbContext db,
            string name,
            string slug,
            string sku,
            string gender,
            string categorySlug,
            string brandSlug,
            decimal basePrice,
            decimal? salePrice,
            string shortDescription,
            string primaryImage,
            string[] colorNames)
        {
            var brandId = db.Brands.Where(item => item.Slug == brandSlug).Select(item => item.Id).First();
            var categoryId = db.Categories.Where(item => item.Slug == categorySlug).Select(item => item.Id).First();

            var product = new Product
            {
                Name = name,
                Slug = slug,
                Sku = sku,
                Gender = gender,
                BrandId = brandId,
                CategoryId = categoryId,
                BasePrice = basePrice,
                SalePrice = salePrice,
                ShortDescription = shortDescription,
                Description = shortDescription + " Built with a durable outsole and premium comfort lining.",
                ThumbnailImage = primaryImage,
                IsFeatured = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.Products.Add(product);
            db.SaveChanges();

            db.ProductImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = primaryImage,
                AltText = name,
                SortOrder = 1,
                IsPrimary = true
            });

            db.ProductImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = primaryImage + "&sat=-100",
                AltText = name + " side view",
                SortOrder = 2,
                IsPrimary = false
            });

            var sizeIds = db.Sizes.OrderBy(item => item.SortOrder).Where(item => item.Name == "39" || item.Name == "40" || item.Name == "41" || item.Name == "42").ToList();
            var variantIndex = 1;
            foreach (var colorName in colorNames)
            {
                var colorId = db.Colors.Where(item => item.Name == colorName).Select(item => item.Id).First();
                foreach (var size in sizeIds)
                {
                    db.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.Id,
                        ColorId = colorId,
                        SizeId = size.Id,
                        Sku = string.Format("{0}-{1}-{2}", sku, colorName.Substring(0, Math.Min(3, colorName.Length)).ToUpperInvariant(), size.Name),
                        Price = salePrice ?? basePrice,
                        StockQuantity = 8 + variantIndex,
                        IsActive = true
                    });
                    variantIndex++;
                }
            }

            db.SaveChanges();
        }

        private static void EnsureBrand(ShopDbContext db, string name, string slug, string description)
        {
            if (!db.Brands.Any(item => item.Slug == slug))
            {
                db.Brands.Add(new Brand
                {
                    Name = name,
                    Slug = slug,
                    Description = description,
                    IsActive = true
                });
            }
        }

        private static void EnsureCategory(ShopDbContext db, string name, string slug, string description)
        {
            if (!db.Categories.Any(item => item.Slug == slug))
            {
                db.Categories.Add(new Category
                {
                    Name = name,
                    Slug = slug,
                    Description = description,
                    IsActive = true
                });
            }
        }

        private static void EnsureColor(ShopDbContext db, string name, string code, string hex)
        {
            if (!db.Colors.Any(item => item.Name == name))
            {
                db.Colors.Add(new Color
                {
                    Name = name,
                    Code = code,
                    HexValue = hex
                });
            }
        }

        private static void SeedUsers(ShopDbContext db)
        {
            EnsureUser(db, "System Administrator", "0900000001", "admin@shop.local", "1 Admin Street", "001122334455", "Admin@123", "Admin");
            EnsureUser(db, "Store Manager", "0900000002", "manager@shop.local", "2 Manager Avenue", "001122334456", "Manager@123", "Manager");
            EnsureUser(db, "Default Customer", "0900000003", "customer@shop.local", "3 Customer Lane", "001122334457", "Customer@123", "Customer");
        }

        private static void EnsureUser(ShopDbContext db, string fullName, string phoneNumber, string email, string address, string citizenId, string password, string roleName)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            if (db.Users.Any(user => user.Email == normalizedEmail))
            {
                return;
            }

            var roleId = db.Roles.Where(role => role.Name == roleName).Select(role => role.Id).First();
            db.Users.Add(new AppUser
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Email = normalizedEmail,
                Address = address,
                CitizenId = citizenId,
                PasswordHash = PasswordHasher.HashPassword(password),
                RoleId = roleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            db.SaveChanges();
        }
    }
}
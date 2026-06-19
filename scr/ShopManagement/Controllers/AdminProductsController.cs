using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index(string keyword, int? brandId, int? categoryId)
        {
            var query = db.Products.Include(item => item.Brand).Include(item => item.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                query = query.Where(item => item.Name.Contains(term) || item.Sku.Contains(term));
            }

            if (brandId.HasValue)
            {
                query = query.Where(item => item.BrandId == brandId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(item => item.CategoryId == categoryId.Value);
            }

            ViewBag.Keyword = keyword;
            ViewBag.BrandId = new SelectList(db.Brands.OrderBy(item => item.Name).ToList(), "Id", "Name", brandId);
            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(item => item.Name).ToList(), "Id", "Name", categoryId);

            var items = query.OrderByDescending(item => item.CreatedAt).ToList();
            return View(items);
        }

        public ActionResult Create()
        {
            BindDropDowns();
            return View(new Product
            {
                IsActive = true,
                IsFeatured = false,
                CreatedAt = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product model)
        {
            ValidateProduct(model, null);
            if (!ModelState.IsValid)
            {
                BindDropDowns(model.BrandId, model.CategoryId);
                return View(model);
            }

            model.Slug = model.Slug.Trim().ToLowerInvariant();
            model.Sku = model.Sku.Trim().ToUpperInvariant();
            model.CreatedAt = DateTime.Now;
            db.Products.Add(model);
            db.SaveChanges();
            TempData["StatusMessage"] = "T\u1EA1o s\u1EA3n ph\u1EA9m th\u00E0nh c\u00F4ng.";
            return RedirectToAction("Edit", new { id = model.Id });
        }

        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var item = db.Products.Find(id.Value);
            if (item == null)
            {
                return HttpNotFound();
            }

            BindDropDowns(item.BrandId, item.CategoryId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model)
        {
            var product = db.Products.Find(model.Id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ValidateProduct(model, model.Id);
            if (!ModelState.IsValid)
            {
                BindDropDowns(model.BrandId, model.CategoryId);
                return View(model);
            }

            product.Name = model.Name.Trim();
            product.Slug = model.Slug.Trim().ToLowerInvariant();
            product.Sku = model.Sku.Trim().ToUpperInvariant();
            product.ShortDescription = model.ShortDescription;
            product.Description = model.Description;
            product.BrandId = model.BrandId;
            product.CategoryId = model.CategoryId;
            product.Gender = model.Gender;
            product.BasePrice = model.BasePrice;
            product.SalePrice = model.SalePrice;
            product.ThumbnailImage = model.ThumbnailImage;
            product.IsFeatured = model.IsFeatured;
            product.IsActive = model.IsActive;
            db.SaveChanges();

            TempData["StatusMessage"] = "C\u1EADp nh\u1EADt s\u1EA3n ph\u1EA9m th\u00E0nh c\u00F4ng.";
            return RedirectToAction("Edit", new { id = product.Id });
        }

        public ActionResult Variants(int? productId)
        {
            if (!productId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.Products
                .Include(item => item.Brand)
                .Include(item => item.Category)
                .Include(item => item.Variants.Select(variant => variant.Color))
                .Include(item => item.Variants.Select(variant => variant.Size))
                .FirstOrDefault(item => item.Id == productId.Value);

            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.ColorId = new SelectList(db.Colors.OrderBy(item => item.Name).ToList(), "Id", "Name");
            ViewBag.SizeId = new SelectList(db.Sizes.OrderBy(item => item.SortOrder).ToList(), "Id", "Name");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddVariant(int productId, int colorId, int sizeId, string sku, decimal price, int stockQuantity, bool isActive = true)
        {
            var product = db.Products.Find(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            var normalizedSku = (sku ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalizedSku))
            {
                TempData["StatusMessage"] = "Vui l\u00F2ng nh\u1EADp SKU.";
                return RedirectToAction("Variants", new { productId = productId });
            }

            if (db.ProductVariants.Any(item => item.Sku == normalizedSku))
            {
                TempData["StatusMessage"] = "SKU bi\u1EBFn th\u1EC3 \u0111\u00E3 t\u1ED3n t\u1EA1i.";
                return RedirectToAction("Variants", new { productId = productId });
            }

            db.ProductVariants.Add(new ProductVariant
            {
                ProductId = productId,
                ColorId = colorId,
                SizeId = sizeId,
                Sku = normalizedSku,
                Price = price,
                StockQuantity = stockQuantity < 0 ? 0 : stockQuantity,
                IsActive = isActive
            });

            db.SaveChanges();
            TempData["StatusMessage"] = "\u0110\u00E3 th\u00EAm bi\u1EBFn th\u1EC3 th\u00E0nh c\u00F4ng.";
            return RedirectToAction("Variants", new { productId = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateVariant(int id, decimal price, int stockQuantity, bool isActive)
        {
            var variant = db.ProductVariants.Find(id);
            if (variant == null)
            {
                return HttpNotFound();
            }

            variant.Price = price;
            variant.StockQuantity = stockQuantity < 0 ? 0 : stockQuantity;
            variant.IsActive = isActive;
            db.SaveChanges();
            TempData["StatusMessage"] = "\u0110\u00E3 c\u1EADp nh\u1EADt bi\u1EBFn th\u1EC3 th\u00E0nh c\u00F4ng.";
            return RedirectToAction("Variants", new { productId = variant.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteVariant(int id)
        {
            var variant = db.ProductVariants.Find(id);
            if (variant == null)
            {
                return HttpNotFound();
            }

            var productId = variant.ProductId;
            db.ProductVariants.Remove(variant);
            db.SaveChanges();
            TempData["StatusMessage"] = "\u0110\u00E3 x\u00F3a bi\u1EBFn th\u1EC3 th\u00E0nh c\u00F4ng.";
            return RedirectToAction("Variants", new { productId = productId });
        }

        private void ValidateProduct(Product model, int? editingId)
        {
            if (db.Products.Any(item => item.Id != editingId && item.Slug == model.Slug))
            {
                ModelState.AddModelError("Slug", "Slug \u0111\u00E3 t\u1ED3n t\u1EA1i.");
            }

            if (db.Products.Any(item => item.Id != editingId && item.Sku == model.Sku))
            {
                ModelState.AddModelError("Sku", "SKU \u0111\u00E3 t\u1ED3n t\u1EA1i.");
            }

            if (model.SalePrice.HasValue && model.SalePrice.Value > model.BasePrice)
            {
                ModelState.AddModelError("SalePrice", "Gi\u00E1 khuy\u1EBFn m\u00E3i ph\u1EA3i nh\u1ECF h\u01A1n ho\u1EB7c b\u1EB1ng gi\u00E1 g\u1ED1c.");
            }
        }

        private void BindDropDowns(int? selectedBrandId = null, int? selectedCategoryId = null)
        {
            ViewBag.BrandId = new SelectList(db.Brands.OrderBy(item => item.Name).ToList(), "Id", "Name", selectedBrandId);
            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(item => item.Name).ToList(), "Id", "Name", selectedCategoryId);
            ViewBag.Gender = new SelectList(new[] { "Men", "Women", "Unisex" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
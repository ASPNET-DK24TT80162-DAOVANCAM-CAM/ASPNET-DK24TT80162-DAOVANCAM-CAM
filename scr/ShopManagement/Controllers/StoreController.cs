using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [AllowAnonymous]
    public class StoreController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index()
        {
            var model = new StoreHomeViewModel
            {
                NewArrivals = db.Products.Include(item => item.Brand).Include(item => item.Category)
                    .Where(item => item.IsActive)
                    .OrderByDescending(item => item.CreatedAt)
                    .Take(8)
                    .ToList(),
                FeaturedMen = db.Products.Include(item => item.Brand).Include(item => item.Category)
                    .Where(item => item.IsActive && item.IsFeatured && (item.Gender == "Men" || item.Gender == "Unisex"))
                    .OrderByDescending(item => item.CreatedAt)
                    .Take(4)
                    .ToList(),
                FeaturedWomen = db.Products.Include(item => item.Brand).Include(item => item.Category)
                    .Where(item => item.IsActive && item.IsFeatured && (item.Gender == "Women" || item.Gender == "Unisex"))
                    .OrderByDescending(item => item.CreatedAt)
                    .Take(4)
                    .ToList()
            };

            return View(model);
        }

        public ActionResult Products(
            string q,
            string gender,
            int? category,
            int? categoryId,
            int? brandId,
            int? colorId,
            int? sizeId,
            string brand,
            string color,
            string size,
            decimal? minPrice,
            decimal? maxPrice,
            string pricePreset,
            string sort,
            int page = 1)
        {
            const int pageSize = 9;
            page = page < 1 ? 1 : page;
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
            sort = NormalizeSort(sort);

            var categoryFilter = category ?? categoryId;
            var selectedBrandIds = ParseMultiIntQuery("brand", brand);
            var selectedColorIds = ParseMultiIntQuery("color", color);
            var selectedSizeIds = ParseMultiIntQuery("size", size);

            if (!selectedBrandIds.Any() && brandId.HasValue)
            {
                selectedBrandIds.Add(brandId.Value);
            }

            if (!selectedColorIds.Any() && colorId.HasValue)
            {
                selectedColorIds.Add(colorId.Value);
            }

            if (!selectedSizeIds.Any() && sizeId.HasValue)
            {
                selectedSizeIds.Add(sizeId.Value);
            }

            ApplyPricePreset(pricePreset, ref minPrice, ref maxPrice);

            if (minPrice.HasValue && minPrice.Value < 0)
            {
                minPrice = 0;
            }

            if (maxPrice.HasValue && maxPrice.Value < 0)
            {
                maxPrice = 0;
            }

            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                var tmp = minPrice;
                minPrice = maxPrice;
                maxPrice = tmp;
            }

            var query = db.Products.Include(item => item.Brand).Include(item => item.Category).Include(item => item.Variants)
                .Where(item => item.IsActive);

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(item =>
                    item.Name.Contains(q) ||
                    item.Sku.Contains(q) ||
                    item.ShortDescription.Contains(q) ||
                    item.Description.Contains(q) ||
                    item.Brand.Name.Contains(q) ||
                    item.Category.Name.Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(gender))
            {
                query = query.Where(item => item.Gender == gender || item.Gender == "Unisex");
            }

            if (categoryFilter.HasValue)
            {
                query = query.Where(item => item.CategoryId == categoryFilter.Value);
            }

            if (selectedBrandIds.Any())
            {
                query = query.Where(item => selectedBrandIds.Contains(item.BrandId));
            }

            if (selectedColorIds.Any())
            {
                query = query.Where(item => item.Variants.Any(variant => variant.IsActive && selectedColorIds.Contains(variant.ColorId)));
            }

            if (selectedSizeIds.Any())
            {
                query = query.Where(item => item.Variants.Any(variant => variant.IsActive && selectedSizeIds.Contains(variant.SizeId)));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(item => (item.SalePrice ?? item.BasePrice) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(item => (item.SalePrice ?? item.BasePrice) <= maxPrice.Value);
            }

            switch (sort)
            {
                case "relevance":
                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        query = query
                            .OrderByDescending(item => item.Name.Contains(q))
                            .ThenByDescending(item => item.ShortDescription != null && item.ShortDescription.Contains(q))
                            .ThenByDescending(item => item.CreatedAt);
                    }
                    else
                    {
                        query = query.OrderByDescending(item => item.CreatedAt);
                    }
                    break;
                case "best_selling":
                    var bestSelling = db.OrderItems
                        .GroupBy(item => item.ProductVariant.ProductId)
                        .Select(group => new
                        {
                            ProductId = group.Key,
                            Quantity = group.Sum(item => item.Quantity)
                        });

                    query = query
                        .GroupJoin(bestSelling,
                            product => product.Id,
                            sold => sold.ProductId,
                            (product, sold) => new
                            {
                                Product = product,
                                Quantity = sold.Select(item => (int?)item.Quantity).FirstOrDefault() ?? 0
                            })
                        .OrderByDescending(item => item.Quantity)
                        .ThenByDescending(item => item.Product.CreatedAt)
                        .Select(item => item.Product);
                    break;
                case "newest":
                    query = query.OrderByDescending(item => item.CreatedAt);
                    break;
                case "price_asc":
                    query = query.OrderBy(item => item.SalePrice ?? item.BasePrice);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(item => item.SalePrice ?? item.BasePrice);
                    break;
                default:
                    query = query.OrderByDescending(item => item.CreatedAt);
                    break;
            }

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var model = new ProductListViewModel
            {
                Products = query.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                Brands = db.Brands.Where(item => item.IsActive).OrderBy(item => item.Name).ToList(),
                Categories = db.Categories.Where(item => item.IsActive).OrderBy(item => item.Name).ToList(),
                Colors = db.Colors.OrderBy(item => item.Name).ToList(),
                Sizes = db.Sizes.OrderBy(item => item.SortOrder).ToList(),
                SelectedBrandIds = selectedBrandIds,
                SelectedColorIds = selectedColorIds,
                SelectedSizeIds = selectedSizeIds,
                QueryText = q,
                Gender = gender,
                Category = categoryFilter,
                CategoryId = categoryFilter,
                BrandId = selectedBrandIds.FirstOrDefault(),
                ColorId = selectedColorIds.FirstOrDefault(),
                SizeId = selectedSizeIds.FirstOrDefault(),
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PricePreset = pricePreset,
                Sort = sort,
                Page = page,
                TotalPages = totalPages == 0 ? 1 : totalPages,
                TotalItems = totalItems
            };

            return View(model);
        }

        public ActionResult Details(string slug, int? variantId)
        {
            var product = db.Products
                .Include(item => item.Brand)
                .Include(item => item.Category)
                .Include(item => item.Images)
                .Include(item => item.Variants.Select(variant => variant.Color))
                .Include(item => item.Variants.Select(variant => variant.Size))
                .FirstOrDefault(item => item.Slug == slug && item.IsActive);

            if (product == null)
            {
                return HttpNotFound();
            }

            var variants = product.Variants.Where(item => item.IsActive).OrderBy(item => item.Color.Name).ThenBy(item => item.Size.SortOrder).ToList();
            var selectedVariant = variants.FirstOrDefault(item => variantId.HasValue && item.Id == variantId.Value)
                                  ?? variants.FirstOrDefault(item => item.StockQuantity > 0)
                                  ?? variants.FirstOrDefault();

            var model = new ProductDetailsViewModel
            {
                Product = product,
                Variants = variants,
                SelectedVariant = selectedVariant,
                RelatedProducts = db.Products.Where(item => item.IsActive && item.Id != product.Id && (item.CategoryId == product.CategoryId || item.BrandId == product.BrandId))
                    .OrderByDescending(item => item.BrandId == product.BrandId)
                    .ThenByDescending(item => item.CategoryId == product.CategoryId)
                    .ThenByDescending(item => item.IsFeatured)
                    .ThenByDescending(item => item.CreatedAt)
                    .Take(4)
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public JsonResult VariantSnapshot(string slug, int variantId)
        {
            if (string.IsNullOrWhiteSpace(slug) || variantId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Y\u00EAu c\u1EA7u kh\u00F4ng h\u1EE3p l\u1EC7."
                }, JsonRequestBehavior.AllowGet);
            }

            var product = db.Products
                .Include(item => item.Variants.Select(variantEntry => variantEntry.Color))
                .Include(item => item.Variants.Select(variantEntry => variantEntry.Size))
                .FirstOrDefault(item => item.IsActive && item.Slug == slug);

            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Kh\u00F4ng t\u00ECm th\u1EA5y s\u1EA3n ph\u1EA9m."
                }, JsonRequestBehavior.AllowGet);
            }

            var selectedVariant = product.Variants.FirstOrDefault(item => item.IsActive && item.Id == variantId);
            if (selectedVariant == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Kh\u00F4ng t\u00ECm th\u1EA5y bi\u1EBFn th\u1EC3."
                }, JsonRequestBehavior.AllowGet);
            }

            var currentPrice = selectedVariant.Price > 0 ? selectedVariant.Price : (product.SalePrice ?? product.BasePrice);

            return Json(new
            {
                success = true,
                variantId = selectedVariant.Id,
                color = selectedVariant.Color.Name,
                size = selectedVariant.Size.Name,
                stock = selectedVariant.StockQuantity,
                inStock = selectedVariant.StockQuantity > 0,
                price = currentPrice,
                priceText = currentPrice.ToString("N0") + " VN\u0110",
                statusText = "\u0110\u00E3 ch\u1ECDn: " + selectedVariant.Color.Name + " / Size " + selectedVariant.Size.Name + " - T\u1ED3n kho: " + selectedVariant.StockQuantity
            }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        private List<int> ParseMultiIntQuery(string key, string csvFallback)
        {
            var values = Request.QueryString.GetValues(key) ?? new string[0];
            var tokens = values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .SelectMany(value => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(value => value.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            if (!tokens.Any() && !string.IsNullOrWhiteSpace(csvFallback))
            {
                tokens = csvFallback
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();
            }

            return tokens
                .Select(value =>
                {
                    int parsed;
                    return int.TryParse(value, out parsed) ? (int?)parsed : null;
                })
                .Where(value => value.HasValue && value.Value > 0)
                .Select(value => value.Value)
                .Distinct()
                .ToList();
        }

        private static string NormalizeSort(string sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
            {
                return "newest";
            }

            var normalized = sort.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "relevance":
                case "best_selling":
                case "newest":
                case "price_asc":
                case "price_desc":
                    return normalized;
                default:
                    return "newest";
            }
        }

        private static void ApplyPricePreset(string pricePreset, ref decimal? minPrice, ref decimal? maxPrice)
        {
            if (minPrice.HasValue || maxPrice.HasValue || string.IsNullOrWhiteSpace(pricePreset))
            {
                return;
            }

            switch (pricePreset.Trim().ToLowerInvariant())
            {
                case "under_500k":
                    minPrice = 0m;
                    maxPrice = 500000m;
                    break;
                case "500k_1m":
                    minPrice = 500000m;
                    maxPrice = 1000000m;
                    break;
                case "1m_2m":
                    minPrice = 1000000m;
                    maxPrice = 2000000m;
                    break;
                case "over_2m":
                    minPrice = 2000000m;
                    maxPrice = null;
                    break;
            }
        }
    }
}
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index()
        {
            var cart = GetOrCreateCart();
            var items = db.CartItems
                .Include(item => item.ProductVariant.Product.Brand)
                .Include(item => item.ProductVariant.Product.Category)
                .Include(item => item.ProductVariant.Color)
                .Include(item => item.ProductVariant.Size)
                .Where(item => item.CartId == cart.Id)
                .OrderBy(item => item.Id)
                .ToList();

            var model = new CartPageViewModel
            {
                Items = items,
                Subtotal = items.Sum(item => item.Quantity * item.UnitPrice)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productVariantId, int quantity = 1, string returnUrl = null)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var variant = db.ProductVariants.Include(entry => entry.Product).FirstOrDefault(entry => entry.Id == productVariantId && entry.IsActive);
            if (variant == null)
            {
                TempData["StatusMessage"] = "Kh\u00F4ng t\u00ECm th\u1EA5y bi\u1EBFn th\u1EC3 s\u1EA3n ph\u1EA9m.";
                return RedirectBack(returnUrl);
            }

            var cart = GetOrCreateCart();
            var cartItem = db.CartItems.FirstOrDefault(entry => entry.CartId == cart.Id && entry.ProductVariantId == productVariantId);
            var nextQuantity = quantity;
            if (cartItem != null)
            {
                nextQuantity = cartItem.Quantity + quantity;
            }

            if (nextQuantity > variant.StockQuantity)
            {
                TempData["StatusMessage"] = "S\u1ED1 l\u01B0\u1EE3ng v\u01B0\u1EE3t qu\u00E1 t\u1ED3n kho hi\u1EC7n t\u1EA1i.";
                return RedirectBack(returnUrl);
            }

            if (cartItem == null)
            {
                db.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = productVariantId,
                    Quantity = quantity,
                    UnitPrice = ResolveVariantUnitPrice(variant)
                });
            }
            else
            {
                cartItem.Quantity = nextQuantity;
            }

            cart.UpdatedAt = DateTime.Now;
            db.SaveChanges();
            TempData["StatusMessage"] = "\u0110\u00E3 th\u00EAm s\u1EA3n ph\u1EA9m v\u00E0o gi\u1ECF h\u00E0ng.";
            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyNow(int productVariantId, int quantity = 1, string returnUrl = null)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var variant = db.ProductVariants.Include(entry => entry.Product).FirstOrDefault(entry => entry.Id == productVariantId && entry.IsActive);
            if (variant == null)
            {
                TempData["StatusMessage"] = "Kh\u00F4ng t\u00ECm th\u1EA5y bi\u1EBFn th\u1EC3 s\u1EA3n ph\u1EA9m.";
                return RedirectBack(returnUrl);
            }

            var cart = GetOrCreateCart();
            var cartItem = db.CartItems.FirstOrDefault(entry => entry.CartId == cart.Id && entry.ProductVariantId == productVariantId);
            var nextQuantity = quantity;
            if (cartItem != null)
            {
                nextQuantity = cartItem.Quantity + quantity;
            }

            if (nextQuantity > variant.StockQuantity)
            {
                TempData["StatusMessage"] = "S\u1ED1 l\u01B0\u1EE3ng v\u01B0\u1EE3t qu\u00E1 t\u1ED3n kho hi\u1EC7n t\u1EA1i.";
                return RedirectBack(returnUrl);
            }

            if (cartItem == null)
            {
                db.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = productVariantId,
                    Quantity = quantity,
                    UnitPrice = ResolveVariantUnitPrice(variant)
                });
            }
            else
            {
                cartItem.Quantity = nextQuantity;
            }

            cart.UpdatedAt = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("Index", "Checkout");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int id, int quantity)
        {
            var cart = GetOrCreateCart();
            var item = db.CartItems.Include(entry => entry.ProductVariant).FirstOrDefault(entry => entry.Id == id && entry.CartId == cart.Id);
            if (item == null)
            {
                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                db.CartItems.Remove(item);
            }
            else
            {
                if (quantity > item.ProductVariant.StockQuantity)
                {
                    TempData["StatusMessage"] = "S\u1ED1 l\u01B0\u1EE3ng v\u01B0\u1EE3t qu\u00E1 t\u1ED3n kho hi\u1EC7n t\u1EA1i.";
                    return RedirectToAction("Index");
                }

                item.Quantity = quantity;
            }

            cart.UpdatedAt = DateTime.Now;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateQuantityAjax(int id, int quantity)
        {
            var cart = GetOrCreateCart();
            var item = db.CartItems
                .Include(entry => entry.ProductVariant.Product)
                .FirstOrDefault(entry => entry.Id == id && entry.CartId == cart.Id);

            if (item == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Kh\u00F4ng t\u00ECm th\u1EA5y s\u1EA3n ph\u1EA9m trong gi\u1ECF h\u00E0ng."
                });
            }

            if (quantity <= 0)
            {
                db.CartItems.Remove(item);
                cart.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                var removedState = BuildCartRealtimeState(cart.Id);
                return Json(new
                {
                    success = true,
                    removed = true,
                    itemId = id,
                    subtotal = removedState.Subtotal,
                    subtotalText = removedState.Subtotal.ToString("N0") + " VN\u0110",
                    hasStockIssue = removedState.HasStockIssue,
                    itemCount = removedState.ItemCount
                });
            }

            if (quantity > item.ProductVariant.StockQuantity)
            {
                return Json(new
                {
                    success = false,
                    message = "S\u1ED1 l\u01B0\u1EE3ng v\u01B0\u1EE3t qu\u00E1 t\u1ED3n kho hi\u1EC7n t\u1EA1i.",
                    stock = item.ProductVariant.StockQuantity
                });
            }

            item.Quantity = quantity;
            cart.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            var state = BuildCartRealtimeState(cart.Id);
            var lineTotal = item.Quantity * item.UnitPrice;

            return Json(new
            {
                success = true,
                removed = false,
                itemId = item.Id,
                quantity = item.Quantity,
                stock = item.ProductVariant.StockQuantity,
                lineTotal = lineTotal,
                lineTotalText = lineTotal.ToString("N0") + " VN\u0110",
                subtotal = state.Subtotal,
                subtotalText = state.Subtotal.ToString("N0") + " VN\u0110",
                hasStockIssue = state.HasStockIssue,
                itemCount = state.ItemCount,
                warning = BuildLineWarning(item.Quantity, item.ProductVariant.StockQuantity)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            var cart = GetOrCreateCart();
            var item = db.CartItems.FirstOrDefault(entry => entry.Id == id && entry.CartId == cart.Id);
            if (item != null)
            {
                db.CartItems.Remove(item);
                cart.UpdatedAt = DateTime.Now;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveAjax(int id)
        {
            var cart = GetOrCreateCart();
            var item = db.CartItems.FirstOrDefault(entry => entry.Id == id && entry.CartId == cart.Id);
            if (item == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Kh\u00F4ng t\u00ECm th\u1EA5y s\u1EA3n ph\u1EA9m trong gi\u1ECF h\u00E0ng."
                });
            }

            db.CartItems.Remove(item);
            cart.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            var state = BuildCartRealtimeState(cart.Id);
            return Json(new
            {
                success = true,
                itemId = id,
                subtotal = state.Subtotal,
                subtotalText = state.Subtotal.ToString("N0") + " VN\u0110",
                hasStockIssue = state.HasStockIssue,
                itemCount = state.ItemCount
            });
        }

        private Cart GetOrCreateCart()
        {
            var userId = GetCurrentUserId();
            var cart = db.Carts.FirstOrDefault(item => item.UserId == userId && !item.IsCheckedOut);
            if (cart != null)
            {
                return cart;
            }

            cart = new Cart
            {
                UserId = userId,
                IsCheckedOut = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            db.Carts.Add(cart);
            db.SaveChanges();
            return cart;
        }

        private int GetCurrentUserId()
        {
            var email = User.Identity.Name;
            return db.Users.Where(item => item.Email == email).Select(item => item.Id).First();
        }

        private ActionResult RedirectBack(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Cart");
        }

        private static decimal ResolveVariantUnitPrice(ProductVariant variant)
        {
            if (variant == null)
            {
                return 0m;
            }

            if (variant.Price > 0)
            {
                return variant.Price;
            }

            if (variant.Product == null)
            {
                return 0m;
            }

            return variant.Product.SalePrice ?? variant.Product.BasePrice;
        }

        private CartRealtimeState BuildCartRealtimeState(int cartId)
        {
            var items = db.CartItems
                .Include(item => item.ProductVariant)
                .Where(item => item.CartId == cartId)
                .ToList();

            return new CartRealtimeState
            {
                Subtotal = items.Sum(item => item.Quantity * item.UnitPrice),
                ItemCount = items.Count,
                HasStockIssue = items.Any(item => item.Quantity > item.ProductVariant.StockQuantity || item.ProductVariant.StockQuantity <= 0)
            };
        }

        private static string BuildLineWarning(int quantity, int stockQuantity)
        {
            if (stockQuantity <= 0 || quantity > stockQuantity)
            {
                return "T\u1ED3n kho hi\u1EC7n kh\u00F4ng \u0111\u1EE7 cho s\u1ED1 l\u01B0\u1EE3ng \u0111\u00E3 ch\u1ECDn.";
            }

            if (stockQuantity <= 3)
            {
                return "S\u1EA3n ph\u1EA9m s\u1EAFp h\u1EBFt h\u00E0ng (c\u00F2n " + stockQuantity + ").";
            }

            return string.Empty;
        }

        private class CartRealtimeState
        {
            public decimal Subtotal { get; set; }

            public int ItemCount { get; set; }

            public bool HasStockIssue { get; set; }
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
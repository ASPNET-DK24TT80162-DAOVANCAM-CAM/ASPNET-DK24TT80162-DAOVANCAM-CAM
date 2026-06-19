using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index()
        {
            var user = GetCurrentUser();
            var cart = GetActiveCart(user.Id);
            var items = GetCartItems(cart.Id);
            if (!items.Any())
            {
                TempData["StatusMessage"] = "Gi\u1ECF h\u00E0ng c\u1EE7a b\u1EA1n \u0111ang tr\u1ED1ng.";
                return RedirectToAction("Index", "Cart");
            }

            var subtotal = items.Sum(item => item.Quantity * item.UnitPrice);
            var shippingFee = subtotal >= 1500000m ? 0m : 30000m;

            var model = new CheckoutViewModel
            {
                ReceiverName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                ShippingAddress = user.Address,
                PaymentMethod = "COD",
                Items = items,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                Total = subtotal + shippingFee
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(CheckoutViewModel model)
        {
            var user = GetCurrentUser();
            var cart = GetActiveCart(user.Id);
            var items = GetCartItems(cart.Id);
            if (!items.Any())
            {
                TempData["StatusMessage"] = "Gi\u1ECF h\u00E0ng c\u1EE7a b\u1EA1n \u0111ang tr\u1ED1ng.";
                return RedirectToAction("Index", "Cart");
            }

            var subtotal = items.Sum(item => item.Quantity * item.UnitPrice);
            var shippingFee = subtotal >= 1500000m ? 0m : 30000m;

            model.Items = items;
            model.Subtotal = subtotal;
            model.ShippingFee = shippingFee;
            model.Total = subtotal + shippingFee;

            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
            {
                ModelState.AddModelError("PaymentMethod", "Vui l\u00F2ng ch\u1ECDn ph\u01B0\u01A1ng th\u1EE9c thanh to\u00E1n.");
            }

            foreach (var cartItem in items)
            {
                if (cartItem.Quantity > cartItem.ProductVariant.StockQuantity)
                {
                    ModelState.AddModelError(string.Empty, "T\u1ED3n kho kh\u00F4ng \u0111\u1EE7 cho s\u1EA3n ph\u1EA9m " + cartItem.ProductVariant.Product.Name + ".");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var tx = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    var order = new Order
                    {
                        OrderCode = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                        UserId = user.Id,
                        ReceiverName = model.ReceiverName.Trim(),
                        PhoneNumber = model.PhoneNumber.Trim(),
                        ShippingAddress = model.ShippingAddress.Trim(),
                        Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim(),
                        Subtotal = subtotal,
                        ShippingFee = shippingFee,
                        DiscountAmount = 0,
                        TotalAmount = subtotal + shippingFee,
                        PaymentMethod = model.PaymentMethod,
                        OrderStatus = "Pending",
                        CreatedAt = DateTime.Now
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    foreach (var cartItem in items)
                    {
                        var currentVariant = db.ProductVariants
                            .Include(entry => entry.Product)
                            .Include(entry => entry.Color)
                            .Include(entry => entry.Size)
                            .FirstOrDefault(entry => entry.Id == cartItem.ProductVariantId && entry.IsActive);

                        if (currentVariant == null)
                        {
                            throw new InvalidOperationException("Bi\u1EBFn th\u1EC3 kh\u00F4ng c\u00F2n kh\u1EA3 d\u1EE5ng.");
                        }

                        if (cartItem.Quantity > currentVariant.StockQuantity)
                        {
                            throw new InvalidOperationException("T\u1ED3n kho kh\u00F4ng \u0111\u1EE7 cho " + currentVariant.Product.Name + " (" + currentVariant.Color.Name + "/" + currentVariant.Size.Name + ").");
                        }

                        currentVariant.StockQuantity -= cartItem.Quantity;

                        db.OrderItems.Add(new OrderItem
                        {
                            OrderId = order.Id,
                            ProductVariantId = cartItem.ProductVariantId,
                            ProductName = cartItem.ProductVariant.Product.Name,
                            ColorName = cartItem.ProductVariant.Color.Name,
                            SizeName = cartItem.ProductVariant.Size.Name,
                            Quantity = cartItem.Quantity,
                            UnitPrice = cartItem.UnitPrice,
                            LineTotal = cartItem.Quantity * cartItem.UnitPrice
                        });
                    }

                    db.CartItems.RemoveRange(items);
                    cart.IsCheckedOut = true;
                    cart.UpdatedAt = DateTime.Now;
                    db.SaveChanges();
                    tx.Commit();

                    TempData["StatusMessage"] = "\u0110\u1EB7t h\u00E0ng th\u00E0nh c\u00F4ng.";
                    return RedirectToAction("Details", "Orders", new { id = order.Id });
                }
                catch (InvalidOperationException ex)
                {
                    tx.Rollback();
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }
                catch
                {
                    tx.Rollback();
                    ModelState.AddModelError(string.Empty, "Kh\u00F4ng th\u1EC3 \u0111\u1EB7t h\u00E0ng l\u00FAc n\u00E0y. Vui l\u00F2ng th\u1EED l\u1EA1i.");
                    return View(model);
                }
            }
        }

        private AppUser GetCurrentUser()
        {
            var email = User.Identity.Name;
            return db.Users.First(item => item.Email == email);
        }

        private Cart GetActiveCart(int userId)
        {
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

        private System.Collections.Generic.List<CartItem> GetCartItems(int cartId)
        {
            return db.CartItems
                .Include(item => item.ProductVariant.Product)
                .Include(item => item.ProductVariant.Color)
                .Include(item => item.ProductVariant.Size)
                .Where(item => item.CartId == cartId)
                .ToList();
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
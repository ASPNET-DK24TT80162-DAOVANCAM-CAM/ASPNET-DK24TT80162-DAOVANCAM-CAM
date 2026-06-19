using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var items = db.WishlistItems
                .Include(item => item.Product.Brand)
                .Include(item => item.Product.Category)
                .Where(item => item.UserId == userId.Value)
                .OrderByDescending(item => item.CreatedAt)
                .ToList();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productId, string returnUrl)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
            }

            var exists = db.WishlistItems.Any(item => item.UserId == userId.Value && item.ProductId == productId);
            if (!exists)
            {
                db.WishlistItems.Add(new WishlistItem
                {
                    UserId = userId.Value,
                    ProductId = productId,
                    CreatedAt = DateTime.Now
                });
                db.SaveChanges();
                TempData["StatusMessage"] = "Added to wishlist.";
            }

            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id, string returnUrl)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var item = db.WishlistItems.FirstOrDefault(entry => entry.Id == id && entry.UserId == userId.Value);
            if (item != null)
            {
                db.WishlistItems.Remove(item);
                db.SaveChanges();
                TempData["StatusMessage"] = "Removed from wishlist.";
            }

            return RedirectBack(returnUrl);
        }

        private int? GetCurrentUserId()
        {
            var email = User.Identity.Name;
            return db.Users.Where(item => item.Email == email).Select(item => (int?)item.Id).FirstOrDefault();
        }

        private ActionResult RedirectBack(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Wishlist");
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
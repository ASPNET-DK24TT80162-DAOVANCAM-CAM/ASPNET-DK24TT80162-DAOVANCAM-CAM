using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        [AllowAnonymous]
        public ActionResult Index()
        {
            var model = new StoreHomeViewModel
            {
                NewArrivals = db.Products
                    .Include(item => item.Brand)
                    .Include(item => item.Category)
                    .Where(item => item.IsActive)
                    .OrderByDescending(item => item.CreatedAt)
                    .Take(8)
                    .ToList(),
                FeaturedMen = db.Products
                    .Include(item => item.Brand)
                    .Include(item => item.Category)
                    .Where(item => item.IsActive && item.IsFeatured && (item.Gender == "Men" || item.Gender == "Unisex"))
                    .OrderByDescending(item => item.CreatedAt)
                    .Take(4)
                    .ToList(),
                FeaturedWomen = db.Products
                    .Include(item => item.Brand)
                    .Include(item => item.Category)
                    .Where(item => item.IsActive && item.IsFeatured && (item.Gender == "Women" || item.Gender == "Unisex"))
                    .OrderByDescending(item => item.CreatedAt)
                    .Take(4)
                    .ToList()
            };

            return View(model);
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var email = User.Identity.Name;
            var user = db.Users.Include(item => item.Role).FirstOrDefault(item => item.Email == email);
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminPanel()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager")]
        public ActionResult ManagerPanel()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager,Customer")]
        public ActionResult CustomerPanel()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
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
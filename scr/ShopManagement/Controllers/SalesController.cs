using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [AllowAnonymous]
    public class SalesController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index()
        {
            var products = db.Products
                .Include(item => item.Brand)
                .Include(item => item.Category)
                .Include(item => item.Variants.Select(variant => variant.Color))
                .Include(item => item.Variants.Select(variant => variant.Size))
                .Where(item => item.IsActive)
                .OrderByDescending(item => item.CreatedAt)
                .ToList();

            return View(products);
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
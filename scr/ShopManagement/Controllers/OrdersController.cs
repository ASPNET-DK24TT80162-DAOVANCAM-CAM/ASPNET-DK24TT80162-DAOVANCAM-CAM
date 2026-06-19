using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index()
        {
            var userId = GetCurrentUserId();
            var orders = db.Orders
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.CreatedAt)
                .ToList();

            return View(orders);
        }

        public ActionResult Details(int id)
        {
            var userId = GetCurrentUserId();
            var order = db.Orders
                .Include(item => item.Items.Select(orderItem => orderItem.ProductVariant.Product))
                .FirstOrDefault(item => item.Id == id && item.UserId == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(new OrderDetailsViewModel { Order = order });
        }

        private int GetCurrentUserId()
        {
            var email = User.Identity.Name;
            return db.Users.Where(item => item.Email == email).Select(item => item.Id).First();
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
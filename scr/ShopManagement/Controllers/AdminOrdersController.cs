using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AdminOrdersController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index(string status, string keyword)
        {
            var query = db.Orders.Include(item => item.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(item => item.OrderStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim();
                query = query.Where(item => item.OrderCode.Contains(term) || item.ReceiverName.Contains(term) || item.PhoneNumber.Contains(term));
            }

            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
            ViewBag.StatusOptions = GetStatusOptions();

            var items = query.OrderByDescending(item => item.CreatedAt).ToList();
            return View(items);
        }

        public ActionResult Details(int id)
        {
            var order = db.Orders
                .Include(item => item.User)
                .Include(item => item.Items.Select(entry => entry.ProductVariant.Product))
                .FirstOrDefault(item => item.Id == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            ViewBag.StatusOptions = new SelectList(GetStatusOptions(), order.OrderStatus);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string orderStatus)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (!GetStatusOptions().Contains(orderStatus))
            {
                TempData["StatusMessage"] = "Tr\u1EA1ng th\u00E1i \u0111\u01A1n h\u00E0ng kh\u00F4ng h\u1EE3p l\u1EC7.";
                return RedirectToAction("Details", new { id = id });
            }

            order.OrderStatus = orderStatus;
            db.SaveChanges();
            TempData["StatusMessage"] = "\u0110\u00E3 c\u1EADp nh\u1EADt tr\u1EA1ng th\u00E1i \u0111\u01A1n h\u00E0ng.";
            return RedirectToAction("Details", new { id = id });
        }

        private static IList<string> GetStatusOptions()
        {
            return new[] { "Pending", "Confirmed", "Shipping", "Completed", "Cancelled" };
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
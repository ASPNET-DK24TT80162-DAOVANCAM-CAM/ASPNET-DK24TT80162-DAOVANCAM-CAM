using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class UserAdminController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index()
        {
            var users = db.Users.Include(item => item.Role)
                .OrderBy(item => item.Role.Name)
                .ThenBy(item => item.FullName)
                .ToList();

            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            BindRoles(0);
            return View(new UserCreateViewModel { IsActive = true });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserCreateViewModel model)
        {
            var normalizedEmail = model.Email == null ? string.Empty : model.Email.Trim().ToLowerInvariant();
            var citizenId = model.CitizenId == null ? string.Empty : model.CitizenId.Trim();

            if (db.Users.Any(item => item.Email == normalizedEmail))
            {
                ModelState.AddModelError("Email", "Email \u0111\u00E3 t\u1ED3n t\u1EA1i.");
            }

            if (db.Users.Any(item => item.CitizenId == citizenId))
            {
                ModelState.AddModelError("CitizenId", "CCCD \u0111\u00E3 t\u1ED3n t\u1EA1i.");
            }

            if (!ModelState.IsValid)
            {
                BindRoles(model.RoleId);
                return View(model);
            }

            db.Users.Add(new AppUser
            {
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Email = normalizedEmail,
                Address = model.Address.Trim(),
                CitizenId = citizenId,
                PasswordHash = Infrastructure.PasswordHasher.HashPassword(model.Password),
                RoleId = model.RoleId,
                IsActive = model.IsActive,
                CreatedAt = System.DateTime.Now
            });

            db.SaveChanges();
            TempData["StatusMessage"] = "T\u1EA1o ng\u01B0\u1EDDi d\u00F9ng th\u00E0nh c\u00F4ng.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id.Value);
            if (user == null)
            {
                return HttpNotFound();
            }

            BindRoles(user.RoleId);
            return View(new UserEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Address = user.Address,
                CitizenId = user.CitizenId,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserEditViewModel model)
        {
            var normalizedEmail = model.Email == null ? string.Empty : model.Email.Trim().ToLowerInvariant();
            var citizenId = model.CitizenId == null ? string.Empty : model.CitizenId.Trim();

            if (db.Users.Any(item => item.Id != model.Id && item.Email == normalizedEmail))
            {
                ModelState.AddModelError("Email", "Email \u0111\u00E3 t\u1ED3n t\u1EA1i.");
            }

            if (db.Users.Any(item => item.Id != model.Id && item.CitizenId == citizenId))
            {
                ModelState.AddModelError("CitizenId", "CCCD \u0111\u00E3 t\u1ED3n t\u1EA1i.");
            }

            if (!ModelState.IsValid)
            {
                BindRoles(model.RoleId);
                return View(model);
            }

            var user = db.Users.Find(model.Id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (string.Equals(user.Email, User.Identity.Name, System.StringComparison.OrdinalIgnoreCase) && !model.IsActive)
            {
                ModelState.AddModelError("IsActive", "B\u1EA1n kh\u00F4ng th\u1EC3 kh\u00F3a t\u00E0i kho\u1EA3n c\u1EE7a ch\u00EDnh m\u00ECnh.");
            }

            if (!ModelState.IsValid)
            {
                BindRoles(model.RoleId);
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();
            user.Email = normalizedEmail;
            user.Address = model.Address.Trim();
            user.CitizenId = citizenId;
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;
            db.SaveChanges();

            TempData["StatusMessage"] = "C\u1EADp nh\u1EADt ng\u01B0\u1EDDi d\u00F9ng th\u00E0nh c\u00F4ng.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (string.Equals(user.Email, User.Identity.Name, System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["StatusMessage"] = "B\u1EA1n kh\u00F4ng th\u1EC3 kh\u00F3a t\u00E0i kho\u1EA3n c\u1EE7a ch\u00EDnh m\u00ECnh.";
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            db.SaveChanges();
            TempData["StatusMessage"] = user.IsActive ? "\u0110\u00E3 m\u1EDF kh\u00F3a ng\u01B0\u1EDDi d\u00F9ng." : "\u0110\u00E3 kh\u00F3a ng\u01B0\u1EDDi d\u00F9ng.";
            return RedirectToAction("Index");
        }

        private void BindRoles(int selectedRoleId)
        {
            ViewBag.RoleId = new SelectList(db.Roles.OrderBy(item => item.Name).ToList(), "Id", "Name", selectedRoleId);
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
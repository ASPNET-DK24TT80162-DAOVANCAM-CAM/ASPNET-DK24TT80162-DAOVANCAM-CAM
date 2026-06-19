using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopManagement.Infrastructure;
using ShopManagement.Models;

namespace ShopManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var email = model.Email.Trim().ToLowerInvariant();
            var user = db.Users.Include(item => item.Role).FirstOrDefault(item => item.Email == email);

            if (user == null || !user.IsActive || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            AuthManager.SignIn(Response, user, model.RememberMe);

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard", "Home");
        }

        public ActionResult Register()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim().ToLowerInvariant();
            var citizenId = model.CitizenId.Trim();

            if (db.Users.Any(item => item.Email == email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (db.Users.Any(item => item.CitizenId == citizenId))
            {
                ModelState.AddModelError("CitizenId", "Citizen ID already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var customerRoleId = db.Roles.Where(item => item.Name == "Customer").Select(item => item.Id).FirstOrDefault();
            if (customerRoleId == 0)
            {
                ModelState.AddModelError(string.Empty, "Customer role is not available.");
                return View(model);
            }

            var user = new AppUser
            {
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Email = email,
                Address = model.Address.Trim(),
                CitizenId = citizenId,
                PasswordHash = PasswordHasher.HashPassword(model.Password),
                RoleId = customerRoleId,
                IsActive = true,
                CreatedAt = System.DateTime.Now
            };

            db.Users.Add(user);
            db.SaveChanges();

            user = db.Users.Include(item => item.Role).First(item => item.Id == user.Id);
            AuthManager.SignIn(Response, user, false);

            return RedirectToAction("Dashboard", "Home");
        }

        [Authorize]
        [ActionName("Profile")]
        public ActionResult ProfileGet()
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Address = user.Address,
                CitizenId = user.CitizenId,
                RoleName = user.Role.Name
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Profile")]
        public ActionResult ProfilePost(ProfileViewModel model)
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var normalizedEmail = model.Email == null ? string.Empty : model.Email.Trim().ToLowerInvariant();
            var citizenId = model.CitizenId == null ? string.Empty : model.CitizenId.Trim();

            if (db.Users.Any(item => item.Id != user.Id && item.Email == normalizedEmail))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (db.Users.Any(item => item.Id != user.Id && item.CitizenId == citizenId))
            {
                ModelState.AddModelError("CitizenId", "Citizen ID already exists.");
            }

            if (!ModelState.IsValid)
            {
                model.RoleName = user.Role.Name;
                model.Id = user.Id;
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();
            user.Email = normalizedEmail;
            user.Address = model.Address.Trim();
            user.CitizenId = citizenId;
            db.SaveChanges();

            user = db.Users.Include(item => item.Role).First(item => item.Id == user.Id);
            AuthManager.SignIn(Response, user, false);
            TempData["StatusMessage"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!PasswordHasher.VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            user.PasswordHash = PasswordHasher.HashPassword(model.NewPassword);
            db.SaveChanges();
            TempData["StatusMessage"] = "Password changed successfully.";
            return RedirectToAction("ChangePassword");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private AppUser GetCurrentUser()
        {
            var email = User.Identity.Name;
            return db.Users.Include(item => item.Role).FirstOrDefault(item => item.Email == email);
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
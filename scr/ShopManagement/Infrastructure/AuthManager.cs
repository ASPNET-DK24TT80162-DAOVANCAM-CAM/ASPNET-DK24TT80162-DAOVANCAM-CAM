using System;
using System.Web;
using System.Web.Security;
using ShopManagement.Models;

namespace ShopManagement.Infrastructure
{
    public static class AuthManager
    {
        public static void SignIn(HttpResponseBase response, AppUser user, bool rememberMe)
        {
            var issuedAt = DateTime.Now;
            var ticket = new FormsAuthenticationTicket(
                1,
                user.Email,
                issuedAt,
                issuedAt.AddMinutes(120),
                rememberMe,
                user.Role.Name,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Path = FormsAuthentication.FormsCookiePath
            };

            if (rememberMe)
            {
                cookie.Expires = ticket.Expiration;
            }

            response.Cookies.Add(cookie);
        }

        public static void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}
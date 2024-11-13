using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;




namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtector _protector;

        public AccountController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _passwordHasher = new PasswordHasher<User>();
            _protector = dataProtectionProvider.CreateProtector("SampleSecureWeb.SecretProtector");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HttpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            HttpContext.Response.Headers["Pragma"] = "no-cache";
            HttpContext.Response.Headers["Expires"] = "0";
            base.OnActionExecuting(context);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user and clear the session
            if (_httpContextAccessor.HttpContext != null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _httpContextAccessor.HttpContext.Session.Clear();  // Clear session data
            }

            return RedirectToAction("Login", "Account");
        }
    


        public IActionResult LandingPage()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = _passwordHasher.HashPassword(user, user.Password);
                string recoverySecret = Guid.NewGuid().ToString();
                string encryptedSecret = _protector.Protect(recoverySecret);
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(user);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);

            if (existingUser != null &&
                _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, password) == PasswordVerificationResult.Success)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username)
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Keeps the session alive after closing the browser
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Set expiration time
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Username", username);  // Set session value
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View();
        }



        public IActionResult Index()
        {
            return View();
        }


        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword)
        {
            var username = HttpContext.Session.GetString("Username");

            if (username != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username);

                if (user != null && _passwordHasher.VerifyHashedPassword(user, user.Password, oldPassword) == PasswordVerificationResult.Success)
                {
                    user.Password = _passwordHasher.HashPassword(user, newPassword);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Old password is incorrect.");
            }
            return View();
        }
    }
}

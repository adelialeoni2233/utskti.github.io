using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using System.Linq;
using System;
using Google.Authenticator;
using System.Text;

namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtector _protector;

        // Constructor yang diperbaiki dengan parameter IDataProtectionProvider
        public AccountController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _passwordHasher = new PasswordHasher<User>();
            _protector = dataProtectionProvider.CreateProtector("SampleSecureWeb.SecretProtector"); 
        }

        private static byte[] ConvertSecretToBytes(string secret, bool secretIsBase32)
        {
            return secretIsBase32 ? Base32Encoding.ToBytes(secret) : Encoding.UTF8.GetBytes(secret);
        }

        // POST: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Session.Clear();
            }

            return RedirectToAction("Login", "Account");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
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

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Logoff()
        {
         
            HttpContext.Session.SetString("UserName", null);
            HttpContext.Session.SetString("IsValidTwoFactorAuthentication", null);

            // Redirect to the login page
            return RedirectToAction("Login");
        }


        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            bool status = false;

            if (existingUser != null &&
                _passwordHasher.VerifyHashedPassword(existingUser, existingUser.Password, password) == PasswordVerificationResult.Success)
            {
                // Generate a valid Base32 secret for 2FA
                string googleAuthKey = Guid.NewGuid().ToString("N").Substring(0, 16);
                string userUniqueKey = Base32Encoding.ToString(Encoding.UTF8.GetBytes(googleAuthKey));

                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("UserUniqueKey", userUniqueKey);

                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                var setupInfo = tfa.GenerateSetupCode("SampleSecureWeb", username, userUniqueKey, true);

                ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                ViewBag.SetupCode = setupInfo.ManualEntryKey;
                status = true;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            ViewBag.Status = status;
            return View();
        }

        // POST: Account/Verify2FA
        [HttpPost]
        public IActionResult Verify2FA(string twoFactorCode)
        {
            string userUniqueKey = HttpContext.Session.GetString("UserUniqueKey");

            if (!string.IsNullOrEmpty(userUniqueKey))
            {
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                bool isValid = tfa.ValidateTwoFactorPIN(userUniqueKey, twoFactorCode);

                if (isValid)
                {
                    
                    HttpContext.Session.SetString("IsValidTwoFactorAuthentication", "true");
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid 2FA code."); 
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "No user session found.");
            }

            return View("Login");
        }
        public IActionResult ChangePassword()
        {
            return View();
        }


        // POST: Account/ChangePassword
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

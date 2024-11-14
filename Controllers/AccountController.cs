using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using SampleSecureWeb.Services;
using SampleSecureWeb.ViewModels;

namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUser _userData;
        private readonly SmtpEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context, IUser userData, SmtpEmailService emailService)
        {
            _context = context;
            _userData = userData;
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel loginViewModel)
        {
            loginViewModel.ReturnUrl ??= Url.Content("~/");

            var user = new User
            {
                Username = loginViewModel.Username,
                Password = loginViewModel.Password
            };

            var loginUser = _userData.Login(user);
            if (loginUser == null)
            {
                ViewBag.Message = "Login tidak valid. Pastikan username dan password benar.";
                return View();
            }

            HttpContext.Session.SetString("Username", loginUser.Username);
            return RedirectToAction("RequestOtp");
        }

        public IActionResult RequestOtp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            string username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Message = "Sesi Anda telah kedaluwarsa. Silakan login kembali.";
                return RedirectToAction("Login");
            }

            // Verifikasi apakah email sesuai dengan email yang terdaftar untuk username tersebut
            var registeredUser = _userData.GetUserByUsername(username);
            if (registeredUser == null || registeredUser.Email != email)
            {
                ViewBag.Message = "Email tidak sesuai dengan yang terdaftar untuk username ini.";
                return View("RequestOtp");
            }

            string otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("UserOtp", otp);

            try
            {
                bool isEmailSent = await _emailService.SendEmailAsync(email, "Your OTP Code", $"Your OTP code is: {otp}");
                ViewBag.Message = isEmailSent 
                    ? "OTP telah dikirim ke email Anda. Periksa email Anda untuk melanjutkan."
                    : "Gagal mengirim OTP. Silakan coba lagi.";

                return isEmailSent ? RedirectToAction("VerifyOtp") : View("RequestOtp");
            }
            catch (Exception)
            {
                ViewBag.Message = "Terjadi kesalahan saat mengirim email OTP. Silakan coba lagi.";
                return View("RequestOtp");
            }
        }

        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string userOtp)
        {
            string username = HttpContext.Session.GetString("Username");
            string sessionOtp = HttpContext.Session.GetString("UserOtp");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(sessionOtp))
            {
                ViewBag.Message = "Sesi verifikasi telah kadaluarsa. Silakan login kembali.";
                return RedirectToAction("Login");
            }

            if (userOtp == sessionOtp)
            {
                HttpContext.Session.Remove("UserOtp");
                HttpContext.Session.Remove("Username");

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Kode OTP tidak valid atau sudah kedaluwarsa.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegistrationViewModel registrationViewModel)
        {
            if (!ModelState.IsValid) return View(registrationViewModel);

            if (_userData.GetUserByUsername(registrationViewModel.Username) != null)
            {
                ViewBag.Message = "Username sudah terdaftar. Gunakan username lain.";
                return View(registrationViewModel);
            }

            var newUser = new User
            {
                Username = registrationViewModel.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(registrationViewModel.Password),
                Email = registrationViewModel.Email,
                RoleName = "User"
            };

            _userData.Registration(newUser);
            ViewBag.Message = "Registrasi berhasil! Silakan login.";
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var username = User.FindFirstValue(ClaimTypes.Name) ?? throw new Exception("User not logged in.");

                var user = _userData.GetUserByUsername(username) ?? throw new Exception("User not found.");

                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
                {
                    ModelState.AddModelError("", "Current password is incorrect.");
                    return View(model);
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                _userData.UpdateUserPassword(user);

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}

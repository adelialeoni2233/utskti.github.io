using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleSecureWeb.Data;
using SampleSecureWeb.Models;
using SampleSecureWeb.ViewModels;

namespace SampleSecureWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUser _userData;
        public AccountController(IUser user)
        {
            _userData = user;
        }


        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel loginViewModel)
        {
            try
            {
                loginViewModel.ReturnUrl = loginViewModel.ReturnUrl ?? Url.Content("~/");

                var user = new User
                {
                    Username = loginViewModel.Username,
                    Password = loginViewModel.Password
                };

                var loginUser = _userData.Login(user);
                if (loginUser == null)
                {
                    ViewBag.Message = "Invalid login attempt.";
                    return View(loginViewModel);
                }

                // Simpan informasi pengguna dalam sesi untuk verifikasi OTP
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Password", user.Password);

                // Kirim OTP ke pengguna (logika pengiriman OTP harus ditambahkan di sini)
                SendOtpToUser(loginUser); // Implementasikan metode ini

                // Arahkan pengguna ke halaman untuk memasukkan OTP
                return RedirectToAction("VerifyOtp", new { returnUrl = loginViewModel.ReturnUrl });
            }
            catch (System.Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View(loginViewModel);
        }

        public IActionResult VerifyOtp(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string otp, string returnUrl)
        {
            string username = HttpContext.Session.GetString("Username");
            string password = HttpContext.Session.GetString("Password");

            if (IsValidOTP(otp)) // Implementasikan logika untuk memverifikasi OTP
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true // Atur sesuai kebutuhan
                    });

                return Redirect(returnUrl ?? "~/");
            }
            else
            {
                ModelState.AddModelError("", "Invalid OTP.");
            }

            return View();
        }

        private bool IsValidOTP(string otp)
        {
            // Implementasikan logika untuk memvalidasi OTP
            // Misalnya, periksa apakah OTP yang dimasukkan sesuai dengan yang dikirim ke pengguna
            return otp == "123456"; // Contoh validasi statis, ganti dengan logika Anda sendiri
        }

        private void SendOtpToUser(User user)
        {
            // Implementasikan logika pengiriman OTP (misalnya, melalui email atau SMS)
            // Simpan OTP untuk validasi selanjutnya
        }

        // ... (metode lain)
    }
}

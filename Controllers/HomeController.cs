using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SampleSecureWeb.Models;
using SampleSecureWeb.Data;
using Microsoft.AspNetCore.Authorization;
using SampleSecureWeb.ViewModels;


namespace SampleSecureWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.username = User.Identity?.Name ?? ""; 
            string[] fruits = new string[] { "Apple", "Banana", "Orange" };
            ViewBag.fruits = fruits;

            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateEmail(string email)
        {
            if (User.Identity?.Name == null)
            {
                return Unauthorized();
            }

            // Find the user in the database based on the logged-in username
            var user = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update the email and save changes
            user.Email = email;
            _context.SaveChanges();

            TempData["Message"] = "Email updated successfully!";
            return RedirectToAction("About");
        }

        public IActionResult About()
        {
            ViewData["Title"] = "About";

            if (User.Identity.IsAuthenticated)
            {
                string username = User.Identity?.Name ?? "Guest";
                ViewData["Username"] = username;

                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                ViewData["Email"] = user?.Email ?? "Email not available";
            }
            else
            {
                ViewData["Message"] = "Please log in to view user information.";
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

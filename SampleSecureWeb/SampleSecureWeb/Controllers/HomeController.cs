using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SampleSecureWeb.Models;

namespace SampleSecureWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {

        ViewBag.username = "Putra Addo Dhaya Abadi";


        List<string> fruits = new List<string> { "Banana", "Orange", "Apple" };


        ViewBag.fruits = fruits;

        return View();
    }

    public IActionResult About()
    {
        ViewData["Title"] = "About page";
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

    public ActionResult Login()
    {
        HttpContext.Session.SetString("UserName", null);
        HttpContext.Session.SetString("IsValidTwoFactorAuthentication", null);
        return View();
    }

}

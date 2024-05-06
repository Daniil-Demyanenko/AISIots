using System.Diagnostics;
using AISIots.DAL;
using Microsoft.AspNetCore.Mvc;
using AISIots.Models;

namespace AISIots.Controllers;

public class HomeController(ILogger<HomeController> _logger, SqliteContext _db) : Controller
{
    public IActionResult Index()
    {
        _db.Add(new Plan() { Title = "gaga" });
        _db.SaveChanges();
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

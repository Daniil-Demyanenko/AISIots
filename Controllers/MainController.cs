using System.Diagnostics;
using AISIots.DAL;
using Microsoft.AspNetCore.Mvc;
using AISIots.Models;
using AISIots.Services;


namespace AISIots.Controllers;

public class MainController(SqliteContext _db) : Controller
{
    public async Task<IActionResult> Index(string? searchString = null, bool isRpdSearch = true)
    {
        var searcher = new FuzzyService(_db);
        
        if (string.IsNullOrEmpty(searchString))
            return View(searcher.GetNewestRpds());
        
        return View(searcher.GetFuzzySorted(searchString,isRpdSearch));
    }

    [HttpPost]
    public async Task<IActionResult> Search()
    {
        await Task.Delay(10);
        return View();
    }

    [HttpPost("UploadFiles")]
    public async Task<IActionResult> UploadFiles(List<IFormFile>? files)
    {
        var excelFiles = await UploadExcelFilesModel.Create(files, _db);

        return View(excelFiles);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
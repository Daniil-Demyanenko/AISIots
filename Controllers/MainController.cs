using System.Diagnostics;
using AISIots.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AISIots.Models;


namespace AISIots.Controllers;

public class MainController(SqliteContext _db) : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Search(string? fileTitle)
    {
        
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
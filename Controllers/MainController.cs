using System.Diagnostics;
using AISIots.DAL;
using Microsoft.AspNetCore.Mvc;
using AISIots.Models;
using AISIots.Models.DbTables;
using AISIots.Services;


namespace AISIots.Controllers;

public class MainController(SqliteContext _db) : Controller
{
    public IActionResult Index(string? searchString = null, bool isRpdSearch = true)
    {
        return View(SearchModel.Create(_db, searchString, isRpdSearch));
    }

    public async Task<IActionResult> EditRpd(int? id = null)
    {
        var rpd = await RpdFinder.FindOrCreateById(id, _db);
        return View(rpd);
    }
    
    public async Task<IActionResult> ViewPlan(int id)
    {
        var plan = await _db.Plans.FindAsync(id);
        return View(plan);
    }

    [HttpPost]
    public IActionResult CheckSave(Rpd rpd)
    {
        if (string.IsNullOrEmpty(rpd.Title?.Trim()))
        {
            ModelState.Remove("Title"); // Костыль, чтоб убрать сообщение по умолчанию
            ModelState.AddModelError("Title", "Поле обязательно для заполнения");
        }
        else if (RpdFinder.IsContainSameTitleDifferentId(rpd.Title, rpd.Id, _db))
            ModelState.AddModelError("Title", "Такая РПД уже существует");

        if (ModelState.IsValid)
        {
            rpd.UpdateDateTime = DateTime.Now;
            _db.Rpds.Update(rpd);
            _db.SaveChanges();
            return View("Index", SearchModel.Create(_db, rpd.Title, isRpdSearch: true));
        }

        return View("EditRpd", rpd);
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
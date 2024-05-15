using System.Diagnostics;
using AISIots.DAL;
using Microsoft.AspNetCore.Mvc;
using AISIots.Models;
using AISIots.Models.DbTables;
using AISIots.Services;
using Microsoft.EntityFrameworkCore;


namespace AISIots.Controllers;

public class MainController(SqliteContext _db) : Controller
{
    public IActionResult Index(string? searchString = null, bool isRpdSearch = true)
    {
        return View(SearchModel.Create(_db, searchString, isRpdSearch));
    }

    public IActionResult ImportExport()
    {
        return View(new ExportJsonModel(_db));
    }

    public async Task<IActionResult> EditRpd(int? id = null)
    {
        var rpd = await DbFinder.FindOrCreateRpdById(id, _db);
        return View(rpd);
    }

    public IActionResult ViewPlan(int id)
    {
        var plan = _db.Plans
            .Include(p => p.PlanBlocks)
            .ThenInclude(pb => pb.BlockSections)
            .ThenInclude(bs => bs.ShortRpds)
            .FirstOrDefault(p => p.Id == id);
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
        else if (DbFinder.IsContainRpdWithSameTitleDifferentId(rpd.Title, rpd.Id, _db))
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

    public IActionResult MissingReport()
    {
        return View(new MissingReportModel(_db));
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
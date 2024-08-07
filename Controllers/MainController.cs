using System.Diagnostics;
using System.Security.Claims;
using AISIots.DAL;
using Microsoft.AspNetCore.Mvc;
using AISIots.Models;
using AISIots.Models.DbTables;
using AISIots.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace AISIots.Controllers;

public class MainController(SqliteContext _db) : Controller
{
    [Authorize]
    public IActionResult Index(string? searchString = null, bool isRpdSearch = true)
    {
        return View(SearchModel.Create(_db, searchString, isRpdSearch));
    }

    [Authorize]
    public IActionResult ImportExport()
    {
        return View(new ExportJsonModel(_db));
    }

    [Authorize]
    public async Task<IActionResult> EditRpd(int? id = null)
    {
        var rpd = await DbHelper.FindOrCreateRpdById(id, _db);
        return View(rpd);
    }

    [Authorize]
    public IActionResult ViewPlan(int id)
    {
        var plan = _db.Plans
            .Include(p => p.PlanBlocks)
            .ThenInclude(pb => pb.DisciplineSections)
            .ThenInclude(bs => bs.ShortRpds)
            .FirstOrDefault(p => p.Id == id);
        return View(plan);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> CheckSave(Rpd rpd)
    {
        if (string.IsNullOrEmpty(rpd.Title?.Trim()))
        {
            ModelState.Remove("Title"); // Костыль, чтоб убрать сообщение по умолчанию
            ModelState.AddModelError("Title", "Поле обязательно для заполнения");
        }
        else if (DbHelper.IsContainRpdWithSameTitleDifferentId(rpd.Title, rpd.Id, _db))
            ModelState.AddModelError("Title", "Такая РПД уже существует");

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        rpd.UpdateDateTime = DateTime.Now;
        _db.Rpds.Update(rpd);

        await _db.SaveChangesAsync();
        return View("Index", SearchModel.Create(_db, rpd.Title, isRpdSearch: true));
    }

    [Authorize]
    public async Task<IActionResult> CheckSaveAs(Rpd rpd)
    {
        if (string.IsNullOrEmpty(rpd.Title?.Trim()))
        {
            ModelState.Remove("Title");
            ModelState.AddModelError("Title", "Поле обязательно для заполнения");
        }
        else if (await _db.Rpds.AnyAsync(r => r.Title.ToLower() == rpd.Title.ToLower()))
            ModelState.AddModelError("Title", "РПД с таким названием уже существует");

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        rpd.UpdateDateTime = DateTime.Now;
        rpd.Id = 0;
        _db.Rpds.Add(rpd);
        await _db.SaveChangesAsync();
        return View("Index", SearchModel.Create(_db, rpd.Title, isRpdSearch: true));
    } 
    
    [Authorize]
    public async Task<IActionResult> DeleteRdp(Rpd? rpd)
    {
        if (rpd != null) _db.Rpds.Remove(rpd);
        await _db.SaveChangesAsync();

        return View("Index", SearchModel.Create(_db, rpd?.Title));
    }
    

    [HttpPost, Authorize]
    public async Task<IActionResult> UploadFiles(List<IFormFile>? files)
    {
        var excelFiles = await UploadExcelFilesModel.Create(files, _db);

        return View(excelFiles);
    }

    [Authorize]
    public IActionResult MissingReport()
    {
        return View(new MissingReportModel(_db));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Login(string? login, string? password, string? confirmPassword)
    {
        LoginModel model = new(_db, login, password, confirmPassword);

        if (!model.SuccessAuth) return View(model);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, login!)
        };
        var userIdentity = new ClaimsIdentity(claims, "login");
        var userPrincipal = new ClaimsPrincipal(userIdentity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            userPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(3)
            });

        return RedirectToAction("Index", "Main");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Main");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
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

//TODO: История изменений
//TODO: Комментарии к РПД
//TODO: Вывод информации о Плане -> добавить офо/зфо, очн/заочн
//TODO: Отчёт -> убрать шифр, исключить дублирование названий
//TODO: Учётка админа добавляет другие учётки
//TODO: Установка ответственных
//TODO: Кнопка удалить у плана
//TODO: Исправить дублирование планов
namespace AISIots.Controllers;

public class MainController(SqliteContext db) : Controller
{
    [Authorize]
    public IActionResult Index(string? searchString = null, bool isRpdSearch = true)
    {
        return View(SearchModel.Create(db, searchString, isRpdSearch));
    }

    [Authorize]
    public IActionResult ImportExport()
    {
        return View(new ExportJsonModel(db));
    }

    [Authorize]
    public async Task<IActionResult> EditRpd(int? id = null)
    {
        var rpd = await DbHelper.FindOrCreateRpdById(id, db);
        return View(rpd);
    }

    [Authorize]
    public IActionResult ViewPlan(int id)
    {
        var plan = db.Plans
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
        else if (DbHelper.IsContainRpdWithSameTitleDifferentId(rpd.Title, rpd.Id, db))
            ModelState.AddModelError("Title", "Такая РПД уже существует");

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        rpd.UpdateDateTime = DateTime.Now;
        db.Rpds.Update(rpd);

        await db.SaveChangesAsync();
        return View("Index", SearchModel.Create(db, rpd.Title, isRpdSearch: true));
    }

    [Authorize]
    public async Task<IActionResult> CheckSaveAs(Rpd rpd)
    {
        if (string.IsNullOrEmpty(rpd.Title?.Trim()))
        {
            ModelState.Remove("Title");
            ModelState.AddModelError("Title", "Поле обязательно для заполнения");
        }
        else if (await db.Rpds.AnyAsync(r => r.Title.ToLower() == rpd.Title.ToLower()))
            ModelState.AddModelError("Title", "РПД с таким названием уже существует");

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        rpd.UpdateDateTime = DateTime.Now;
        rpd.Id = 0;
        db.Rpds.Add(rpd);
        await db.SaveChangesAsync();
        return View("Index", SearchModel.Create(db, rpd.Title, isRpdSearch: true));
    }

    [Authorize]
    public async Task<IActionResult> DeleteRdp(Rpd? rpd)
    {
        rpd = await db.Rpds.FindAsync(rpd?.Id);
        if (rpd is not null) db.Rpds.Remove(rpd);
        await db.SaveChangesAsync();

        return View("Index", SearchModel.Create(db, rpd?.Title));
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UploadFiles(List<IFormFile>? files)
    {
        var excelFiles = await UploadExcelFilesModel.Create(files, db);

        return View(excelFiles);
    }

    [Authorize]
    public IActionResult MissingReport()
    {
        return View(new MissingReportModel(db));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Login(string? login, string? password, string? confirmPassword)
    {
        LoginModel model = new(db, login, password, confirmPassword);

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

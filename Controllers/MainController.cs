using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AISIots.Models;
using AISIots.ViewModels;
using AISIots.Models.DbTables;
using AISIots.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

//TODO: История изменений
//TODO: Комментарии к РПД
//TODO: Вывод информации о Плане -> добавить офо/зфо, очн/заочн
//TODO: Отчёт -> убрать шифр, исключить дублирование названий
//TODO: Учётка админа добавляет другие учётки
//TODO: Установка ответственных
//TODO: Кнопка удалить у плана
//TODO: Исправить дублирование планов
namespace AISIots.Controllers;

public class MainController(
    IPlanService planService,
    IReportService reportService,
    IFileProcessingService fileProcessingService,
    IAuthService authService) : Controller
{
    [Authorize]
    public IActionResult Index(string? searchString = null, bool isRpdSearch = true)
    {
        return View(planService.Search(searchString, isRpdSearch));
    }

    [Authorize]
    public IActionResult ImportExport()
    {
        return View(reportService.GetExportJson());
    }

    [Authorize]
    public async Task<IActionResult> EditRpd(int? id = null)
    {
        var rpd = await planService.FindOrCreateRpdByIdAsync(id);
        return View(rpd);
    }

    [Authorize]
    public IActionResult ViewPlan(int id)
    {
        var plan = planService.GetPlanById(id);
        return View(plan);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> CheckSave(Rpd rpd)
    {
        var errorMessage = await planService.UpdateRpdAsync(rpd);
        if (errorMessage != null)
        {
            if (errorMessage == "Поле обязательно для заполнения")
                ModelState.Remove("Title");
                
            ModelState.AddModelError("Title", errorMessage);
        }

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        return View("Index", planService.Search(rpd.Title, isRpdSearch: true));
    }

    [Authorize]
    public async Task<IActionResult> CheckSaveAs(Rpd rpd)
    {
        var errorMessage = await planService.CreateRpdAsync(rpd);
        if (errorMessage != null)
        {
            if (errorMessage == "Поле обязательно для заполнения")
                ModelState.Remove("Title");
                
            ModelState.AddModelError("Title", errorMessage);
        }

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        return View("Index", planService.Search(rpd.Title, isRpdSearch: true));
    }

    [Authorize]
    public async Task<IActionResult> DeleteRdp(Rpd? rpd)
    {
        await planService.DeleteRpdAsync(rpd?.Id);
        return View("Index", planService.Search(rpd?.Title, isRpdSearch: true));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UploadFiles(List<IFormFile>? files)
    {
        var excelFiles = await fileProcessingService.ProcessUploadedFilesAsync(files);
        return View(excelFiles);
    }

    [Authorize]
    public async Task<IActionResult> MissingReport()
    {
        return View(await reportService.GetMissingRpdsReportAsync());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Login(string? login, string? password, string? confirmPassword)
    {
        var model = authService.ProcessLogin(login, password, confirmPassword);

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

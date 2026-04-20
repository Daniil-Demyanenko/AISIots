using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AISIots.ViewModels;
using AISIots.Models.DbTables;
using AISIots.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

//TODO: Комментарии к РПД
//TODO: Вывод информации о Плане -> добавить офо/зфо, очн/заочн
//TODO: Отчёт -> убрать шифр, исключить дублирование названий
//TODO: Установка ответственных
//TODO: Исправить дублирование планов
namespace AISIots.Controllers;

public class MainController(
    IPlanService planService,
    IReportService reportService,
    IFileProcessingService fileProcessingService,
    IAuthService authService,
    IActionLogService logService,
    IUserService userService) : Controller
{
    [Authorize]
    public async Task<IActionResult> Index(string? searchString = null, bool isRpdSearch = true)
    {
        return View(await planService.Search(searchString, isRpdSearch));
    }

    [Authorize]
    public async Task<IActionResult> ImportExport()
    {
        return View(await reportService.GetExportJsonAsync());
    }

    [Authorize]
    public async Task<IActionResult> EditRpd(int? id = null)
    {
        var rpd = await planService.FindOrCreateRpdByIdAsync(id);
        if (id.HasValue && rpd.Id == id.Value)
        {
            var userName = User.Identity!.Name;
            await logService.LogActionAsync(userName!, "Просмотр", "РПД", rpd.Title);
        }

        return View(rpd);
    }

    [Authorize]
    public async Task<IActionResult> ViewPlan(int id)
    {
        var plan = await planService.GetPlanByIdOrThrowAsync(id);
        var userName = User.Identity!.Name;
        await logService.LogActionAsync(userName!, "Просмотр", "План", plan.Profile);
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
            return View("EditRpd", rpd);
        }

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        var userName = User.Identity!.Name;
        await logService.LogActionAsync(userName!, "Изменение", "РПД", rpd.Title);
        return RedirectToAction("Index", new { searchString = rpd.Title, isRpdSearch = true });
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
            return View("EditRpd", rpd);
        }

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        var userName = User.Identity!.Name;
        await logService.LogActionAsync(userName!, "Создание", "РПД", rpd.Title);
        return RedirectToAction("Index", new { searchString = rpd.Title, isRpdSearch = true });
    }

    [Authorize]
    public async Task<IActionResult> DeleteRdp(int id)
    {
        var rpd = await planService.FindOrCreateRpdByIdAsync(id);
        if (rpd == null) return NotFound();

        await planService.DeleteRpdAsync(id);
        var userName = User.Identity!.Name;
        await logService.LogActionAsync(userName!, "Удаление", "РПД", rpd.Title);

        return RedirectToAction("Index", new { isRpdSearch = true });
    }

    [Authorize]
    public async Task<IActionResult> DeletePlan(int id)
    {
        var plan = await planService.GetPlanByIdOrThrowAsync(id);
        await planService.DeletePlanAsync(id);
        var userName = User.Identity!.Name;
        await logService.LogActionAsync(userName!, "Удаление", "План", plan.Profile);
        return RedirectToAction("Index", new { isRpdSearch = false });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UploadFiles(List<IFormFile>? files)
    {
        var excelFiles = await fileProcessingService.ProcessUploadedFilesAsync(files);
        var userName = User.Identity!.Name;
        await logService.LogActionAsync(userName!, "Загрузка файлов", "Импорт", $"Файлов: {files?.Count ?? 0}");
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
        var model = await authService.ProcessLogin(login, password, confirmPassword);

        if (!model.SuccessAuth) return View(model);

        var userPrincipal = authService.CreatePrincipal(login!);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            userPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(3)
            });

        await logService.LogActionAsync(login!, "Вход", "Сессия", "Успешный вход");

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

    [Authorize(Roles = "MainAdmin,Admin")]
    public async Task<IActionResult> ChangeUserRole(string login, int newRoleId)
    {
        var userName = User.Identity!.Name;
        await userService.ChangeUserRoleAsync(login, newRoleId, userName!);
        return RedirectToAction("Settings", new { message = $"Роль пользователя {login} изменена" });
    }

    [Authorize]
    public async Task<IActionResult> Settings(string? message = null, int page = 1)
    {
        var userName = User.Identity!.Name;
        var currentUserRole = await authService.GetUserRole(userName!);
        var isAdmin = currentUserRole == "MainAdmin" || currentUserRole == "Admin";
        const int pageSize = 25;

        var model = new SettingsModel
        {
            IsAdmin = isAdmin,
            CurrentUserLogin = userName!,
            CurrentUserRole = !string.IsNullOrEmpty(currentUserRole) ? currentUserRole : "User",
            CurrentPage = page
        };

        if (isAdmin)
        {
            model.Users = await userService.GetAllUsersAsync(userName!);
            var allLogs = await logService.GetLogsAsync();
            model.Logs = allLogs;
            model.TotalPages = (int)Math.Ceiling(allLogs.Count / (double)pageSize);
            model.LogsPage = allLogs.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        else
        {
            var allLogs = await logService.GetLogsByUserAsync(userName!);
            model.Logs = allLogs;
            model.TotalPages = (int)Math.Ceiling(allLogs.Count / (double)pageSize);
            model.LogsPage = allLogs.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        if (!string.IsNullOrEmpty(message))
        {
            model.Message = message;
        }

        return View(model);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
    {
        var userName = User.Identity!.Name;
        var minPassLen = 6;

        if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            return RedirectToAction("Settings", new { message = "Заполните все поля" });
        }

        if (newPassword.Length < minPassLen)
        {
            return RedirectToAction("Settings", new { message = "Пароль должен быть не менее 6 символов" });
        }

        if (newPassword != confirmPassword)
        {
            return RedirectToAction("Settings", new { message = "Пароли не совпадают" });
        }

        var success = await userService.ChangePasswordAsync(userName!, oldPassword, newPassword);
        if (!success)
        {
            return RedirectToAction("Settings", new { message = "Неверный текущий пароль" });
        }

        await logService.LogActionAsync(userName!, "Смена пароля", "Пользователь", userName!);
        return RedirectToAction("Settings", new { message = "Пароль успешно изменён" });
    }
}
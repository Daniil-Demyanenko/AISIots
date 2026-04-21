using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AISIots.ViewModels;
using AISIots.Models.DbTables;
using AISIots.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace AISIots.Controllers;

public class MainController : Controller
{
    private readonly IPlanService _planService;
    private readonly IReportService _reportService;
    private readonly IFileProcessingService _fileProcessingService;
    private readonly IAuthService _authService;
    private readonly IActionLogService _logService;
    private readonly IUserService _userService;
    private readonly ITemplateGeneratorService _templateGeneratorService;
    
    public MainController(
        IPlanService planService,
        IReportService reportService,
        IFileProcessingService fileProcessingService,
        IAuthService authService,
        IActionLogService logService,
        IUserService userService,
        ITemplateGeneratorService templateGeneratorService)
    {
        _planService = planService;
        _reportService = reportService;
        _fileProcessingService = fileProcessingService;
        _authService = authService;
        _logService = logService;
        _userService = userService;
        _templateGeneratorService = templateGeneratorService;
    }
    [Authorize]
    public async Task<IActionResult> Index(string? searchString = null, bool isRpdSearch = true)
    {
        return View(await _planService.Search(searchString, isRpdSearch));
    }

    [Authorize]
    public async Task<IActionResult> ImportExport()
    {
        return View(await _reportService.GetExportJsonAsync());
    }

    [Authorize]
    public async Task<IActionResult> EditRpd(int? id = null)
    {
        var rpd = await _planService.FindOrCreateRpdByIdAsync(id);
        if (id.HasValue && rpd.Id == id.Value)
        {
            var userName = User.Identity!.Name;
            await _logService.LogActionAsync(userName!, "Просмотр", "РПД", rpd.Title);
        }

        return View(rpd);
    }

    [Authorize]
    public async Task<IActionResult> ViewPlan(int id)
    {
        var plan = await _planService.GetPlanByIdOrThrowAsync(id);
        var userName = User.Identity!.Name;
        await _logService.LogActionAsync(userName!, "Просмотр", "План", plan.Profile);
        return View(plan);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> CheckSave(Rpd rpd)
    {
        var errorMessage = await _planService.UpdateRpdAsync(rpd);
        if (errorMessage != null)
        {
            if (errorMessage == "Поле обязательно для заполнения")
                ModelState.Remove("Title");

            ModelState.AddModelError("Title", errorMessage);
            return View("EditRpd", rpd);
        }

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        var userName = User.Identity!.Name;
        await _logService.LogActionAsync(userName!, "Изменение", "РПД", rpd.Title);
        return RedirectToAction("Index", new { searchString = rpd.Title, isRpdSearch = true });
    }

    [Authorize]
    public async Task<IActionResult> CheckSaveAs(Rpd rpd)
    {
        var errorMessage = await _planService.CreateRpdAsync(rpd);
        if (errorMessage != null)
        {
            if (errorMessage == "Поле обязательно для заполнения")
                ModelState.Remove("Title");

            ModelState.AddModelError("Title", errorMessage);
            return View("EditRpd", rpd);
        }

        if (!ModelState.IsValid) return View("EditRpd", rpd);

        var userName = User.Identity!.Name;
        await _logService.LogActionAsync(userName!, "Создание", "РПД", rpd.Title);
        return RedirectToAction("Index", new { searchString = rpd.Title, isRpdSearch = true });
    }

    [Authorize]
    public async Task<IActionResult> DeleteRdp(int id)
    {
        var rpd = await _planService.FindOrCreateRpdByIdAsync(id);
        if (rpd == null) return NotFound();

        await _planService.DeleteRpdAsync(id);
        var userName = User.Identity!.Name;
        await _logService.LogActionAsync(userName!, "Удаление", "РПД", rpd.Title);

        return RedirectToAction("Index", new { isRpdSearch = true });
    }

    [Authorize]
    public async Task<IActionResult> DeletePlan(int id)
    {
        var plan = await _planService.GetPlanByIdOrThrowAsync(id);
        await _planService.DeletePlanAsync(id);
        var userName = User.Identity!.Name;
        await _logService.LogActionAsync(userName!, "Удаление", "План", plan.Profile);
        return RedirectToAction("Index", new { isRpdSearch = false });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UploadFiles(List<IFormFile>? files)
    {
        var excelFiles = await _fileProcessingService.ProcessUploadedFilesAsync(files);
        var userName = User.Identity!.Name;
        await _logService.LogActionAsync(userName!, "Загрузка файлов", "Импорт", $"Файлов: {files?.Count ?? 0}");
        return View(excelFiles);
    }

    [Authorize]
    public async Task<IActionResult> MissingReport()
    {
        return View(await _reportService.GetMissingRpdsReportAsync());
    }

    [Authorize]
    public async Task<IActionResult> DownloadFos(int id)
    {
        var rpd = await _planService.FindOrCreateRpdByIdAsync(id);
        if (rpd == null)
        {
            return NotFound();
        }

        (Stream stream, string fileName) = await _templateGeneratorService.GenerateDocumentAsync(rpd, "fos.docx");
        var userName = User.Identity!.Name;
        await _logService.LogActionAsync(userName!, "Скачать ФОС", "РПД", rpd.Title);

        return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }

    [Authorize]
    public async Task<IActionResult> DownloadRpd(int id)
    {
        var rpd = await _planService.FindOrCreateRpdByIdAsync(id);
        if (rpd == null)
        {
            return NotFound();
        }

        (Stream stream, string fileName) = await _templateGeneratorService.GenerateDocumentAsync(rpd, "rpd.docx");
        var userName = User.Identity!.Name;
        await _logService.LogActionAsync(userName!, "Скачать РПД", "РПД", rpd.Title);

        return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Login(string? login, string? password, string? confirmPassword)
    {
        var model = await _authService.ProcessLogin(login, password, confirmPassword);

        if (!model.SuccessAuth) return View(model);

        var userPrincipal = _authService.CreatePrincipal(login!);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            userPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(3)
            });

        await _logService.LogActionAsync(login!, "Вход", "Сессия", "Успешный вход");

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
        await _userService.ChangeUserRoleAsync(login, newRoleId, userName!);
        return RedirectToAction("Settings", new { message = $"Роль пользователя {login} изменена" });
    }

    [Authorize]
    public async Task<IActionResult> Settings(string? message = null, int page = 1)
    {
        var userName = User.Identity!.Name;
        var currentUserRole = await _authService.GetUserRole(userName!);
        var isAdmin = currentUserRole == "MainAdmin" || currentUserRole == "Admin";
        const int pageSize = 20;

        var model = new SettingsModel
        {
            IsAdmin = isAdmin,
            CurrentUserLogin = userName!,
            CurrentUserRole = !string.IsNullOrEmpty(currentUserRole) ? currentUserRole : "User",
            CurrentPage = page
        };

        if (isAdmin)
        {
            model.Users = await _userService.GetAllUsersAsync(userName!);
            int totalLogs = await _logService.GetLogsCountAsync();
            model.TotalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);
            model.LogsPage = await _logService.GetLogsAsync(page, pageSize);
        }
        else
        {
            int totalLogs = await _logService.GetLogsCountByUserAsync(userName!);
            model.TotalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);
            model.LogsPage = await _logService.GetLogsByUserAsync(userName!, page, pageSize);
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

        var success = await _userService.ChangePasswordAsync(userName!, oldPassword, newPassword);
        if (!success)
        {
            return RedirectToAction("Settings", new { message = "Неверный текущий пароль" });
        }

        await _logService.LogActionAsync(userName!, "Смена пароля", "Пользователь", userName!);
        return RedirectToAction("Settings", new { message = "Пароль успешно изменён" });
    }
}
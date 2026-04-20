using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AISIots.Interfaces;

namespace AISIots.Controllers;

public class UserController(
    IUserService userService,
    IActionLogService logService) : Controller
{
    [HttpPost, Authorize(Roles = "MainAdmin,Admin")]
    public async Task<IActionResult> CreateUser(string login, string password, string confirmPassword, int roleId)
    {
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            return RedirectToAction("Settings", "Main", new { message = "Заполните все поля" });
        }

        if (password != confirmPassword)
        {
            return RedirectToAction("Settings", "Main", new { message = "Пароли не совпадают" });
        }

        try
        {
            var userName = User.Identity!.Name;
            await userService.CreateUserAsync(login, password, roleId, userName!);
            return RedirectToAction("Settings", "Main", new { message = $"Пользователь {login} создан" });
        }
        catch (InvalidOperationException ex)
        {
            return RedirectToAction("Settings", "Main", new { message = ex.Message });
        }
    }

    [HttpPost, Authorize(Roles = "MainAdmin,Admin")]
    public async Task<IActionResult> ChangeUserRole(string login, int newRoleId)
    {
        var userName = User.Identity!.Name;
        await userService.ChangeUserRoleAsync(login, newRoleId, userName!);
        return RedirectToAction("Settings", "Main", new { message = $"Роль пользователя {login} изменена" });
    }

    [HttpPost, Authorize(Roles = "MainAdmin,Admin")]
    public async Task<IActionResult> ChangeUserPassword(string login, string newPassword)
    {
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
        {
            return RedirectToAction("Settings", "Main", new { message = "Пароль должен быть не менее 6 символов" });
        }

        var currentUser = User.Identity!.Name;
        if (!userService.CanEditUser(login, currentUser!))
        {
            return RedirectToAction("Settings", "Main", new { message = "Нельзя редактировать этого пользователя" });
        }

        await userService.ChangeUserPasswordAsync(login, newPassword, currentUser!);
        return RedirectToAction("Settings", "Main", new { message = $"Пароль пользователя {login} изменён" });
    }

    [HttpPost, Authorize(Roles = "MainAdmin,Admin")]
    public async Task<IActionResult> DeleteUser(string login)
    {
        var currentUser = User.Identity!.Name;
        if (!userService.CanEditUser(login, currentUser!))
        {
            return RedirectToAction("Settings", "Main", new { message = "Нельзя удалить этого пользователя" });
        }

        await userService.DeleteUserAsync(login, currentUser!);
        return RedirectToAction("Settings", "Main", new { message = $"Пользователь {login} удалён" });
    }
}
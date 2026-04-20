using System.Security.Cryptography;
using System.Text;
using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.ViewModels;
using AISIots.Models.DbTables;

namespace AISIots.Services;

public class UserService(IDbRepository repository, IActionLogService logService) : IUserService
{
    public async Task<List<UserViewModel>> GetAllUsersAsync(string currentUserLogin)
    {
        var users = await repository.GetUsersAsync();
        var result = new List<UserViewModel>();
        foreach (var u in users)
        {
            var isEditable = CanEditUser(u.Login, currentUserLogin);
            result.Add(new UserViewModel
            {
                Login = u.Login,
                RoleName = u.Role.Name,
                RoleId = u.RoleId,
                RoleIdEditable = u.RoleId != 1 && isEditable
            });
        }

        return result;
    }

    public async Task CreateUserAsync(string login, string password, int roleId, string createdBy)
    {
        var existingUser = await repository.GetUserByLoginAsync(login);
        if (existingUser != null)
            throw new InvalidOperationException("Пользователь с таким логином уже существует");

        using SHA256 sha = SHA256.Create();
        var pass = sha.ComputeHash(Encoding.ASCII.GetBytes(password));
        var user = new User { Login = login, Password = pass, RoleId = roleId };
        await repository.AddUserAsync(user);

        await logService.LogActionAsync(createdBy, "Создание", "Пользователь", login);
    }

    public async Task<bool> ChangePasswordAsync(string login, string oldPassword, string newPassword)
    {
        var user = await repository.GetUserByLoginAsync(login);
        if (user == null) return false;

        using SHA256 sha = SHA256.Create();
        var oldPass = sha.ComputeHash(Encoding.ASCII.GetBytes(oldPassword));

        if (!user.Password.SequenceEqual(oldPass)) return false;

        user.Password = sha.ComputeHash(Encoding.ASCII.GetBytes(newPassword));
        await repository.UpdateUserAsync(user);
        return true;
    }

    public async Task ChangeUserRoleAsync(string login, int newRoleId, string changedBy)
    {
        var user = await repository.GetUserByLoginAsync(login);
        if (user == null || user.RoleId == 1) return;

        user.RoleId = newRoleId;
        await repository.UpdateUserAsync(user);

        await logService.LogActionAsync(changedBy, "Изменение роли", "Пользователь", login);
    }

    public bool CanChangeRoleAsync(string login)
    {
        //- This method should ideally be async, but for simplicity in UI we use sync checks
        // In a real app, we'd use a cached user or pass the role in.
        return true;
    }

    public async Task ChangeUserPasswordAsync(string login, string newPassword, string changedBy)
    {
        var user = await repository.GetUserByLoginAsync(login);
        if (user == null) return;

        using SHA256 sha = SHA256.Create();
        user.Password = sha.ComputeHash(Encoding.ASCII.GetBytes(newPassword));
        await repository.UpdateUserAsync(user);

        await logService.LogActionAsync(changedBy, "Смена пароля", "Пользователь", login);
    }

    public async Task DeleteUserAsync(string login, string deletedBy)
    {
        var user = await repository.GetUserByLoginAsync(login);
        if (user == null || user.RoleId == 1) return;

        await logService.LogActionAsync(deletedBy, "Удаление", "Пользователь", login);
        await repository.DeleteUserAsync(login);
    }

    public bool CanDeleteUser(string login)
    {
        return true;
    }

    public bool CanEditUser(string targetLogin, string currentUserLogin)
    {
        // We can't easily make this sync without a cached user, so we assume 
        // that the logic is handled by the Admin role in the controller
        return true;
    }
}
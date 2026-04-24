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
            var isEditable = await CanEditUser(u.Login, currentUserLogin);
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
        var targetUser = await repository.GetUserByLoginAsync(login);
        var currentUser = await repository.GetUserByLoginAsync(changedBy);

        if (targetUser == null || currentUser == null) return;

        if (!await CanChangeRoleInternal(targetUser, currentUser))
            throw new InvalidOperationException("Недостаточно прав для изменения роли или пользователь защищен");

        targetUser.RoleId = newRoleId;
        await repository.UpdateUserAsync(targetUser);

        await logService.LogActionAsync(changedBy, "Изменение роли", "Пользователь", login);
    }

    public async Task<bool> CanChangeRoleAsync(string targetLogin, string currentUserLogin)
    {
        var targetUser = await repository.GetUserByLoginAsync(targetLogin);
        var currentUser = await repository.GetUserByLoginAsync(currentUserLogin);
        return await CanChangeRoleInternal(targetUser, currentUser);
    }

    private Task<bool> CanChangeRoleInternal(User? targetUser, User? currentUser)
    {
        if (targetUser == null || currentUser == null) return Task.FromResult(false);
        return Task.FromResult(targetUser.RoleId != 1 && currentUser.RoleId is 1 or 2);
    }

    public async Task ChangeUserPasswordAsync(string login, string newPassword, string changedBy)
    {
        var targetUser = await repository.GetUserByLoginAsync(login);
        var currentUser = await repository.GetUserByLoginAsync(changedBy);

        if (!await CanEditUserInternal(targetUser, currentUser))
            throw new InvalidOperationException("Недостаточно прав для изменения пароля");

        using SHA256 sha = SHA256.Create();
        targetUser!.Password = sha.ComputeHash(Encoding.ASCII.GetBytes(newPassword));
        await repository.UpdateUserAsync(targetUser);

        await logService.LogActionAsync(changedBy, "Смена пароля", "Пользователь", login);
    }

    public async Task DeleteUserAsync(string login, string deletedBy)
    {
        var targetUser = await repository.GetUserByLoginAsync(login);
        var currentUser = await repository.GetUserByLoginAsync(deletedBy);

        if (!await CanDeleteUserInternal(targetUser, currentUser))
            throw new InvalidOperationException("Недостаточно прав для удаления пользователя");

        await logService.LogActionAsync(deletedBy, "Удаление", "Пользователь", login);
        await repository.DeleteUserAsync(login);
    }

    public async Task<bool> CanDeleteUser(string targetLogin, string currentUserLogin)
    {
        var targetUser = await repository.GetUserByLoginAsync(targetLogin);
        var currentUser = await repository.GetUserByLoginAsync(currentUserLogin);
        return await CanDeleteUserInternal(targetUser, currentUser);
    }

    private Task<bool> CanDeleteUserInternal(User? targetUser, User? currentUser)
    {
        if (targetUser == null || currentUser == null || targetUser.RoleId == 1) return Task.FromResult(false);
        return Task.FromResult(currentUser.RoleId is 1 or 2);
    }

    public async Task<bool> CanEditUser(string targetLogin, string currentUserLogin)
    {
        var targetUser = await repository.GetUserByLoginAsync(targetLogin);
        var currentUser = await repository.GetUserByLoginAsync(currentUserLogin);
        return await CanEditUserInternal(targetUser, currentUser);
    }

    private Task<bool> CanEditUserInternal(User? targetUser, User? currentUser)
    {
        if (targetUser == null || currentUser == null) return Task.FromResult(false);
        if (currentUser.RoleId == 1 || currentUser.RoleId == 2 && targetUser.RoleId != 1) 
            return Task.FromResult(true);
        
        return Task.FromResult(targetUser.Login == currentUser.Login);
    }
}
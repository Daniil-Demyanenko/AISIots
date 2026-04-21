using AISIots.ViewModels;

namespace AISIots.Interfaces;

public interface IUserService
{
    Task<List<UserViewModel>> GetAllUsersAsync(string currentUserLogin);
    Task CreateUserAsync(string login, string password, int roleId, string createdBy);
    Task<bool> ChangePasswordAsync(string login, string oldPassword, string newPassword);
    Task ChangeUserRoleAsync(string login, int newRoleId, string changedBy);
    Task ChangeUserPasswordAsync(string login, string newPassword, string changedBy);
    Task DeleteUserAsync(string login, string deletedBy);
    Task<bool> CanChangeRoleAsync(string targetLogin, string currentUserLogin);
    Task<bool> CanDeleteUser(string targetLogin, string currentUserLogin);
    Task<bool> CanEditUser(string targetLogin, string currentUserLogin);
}
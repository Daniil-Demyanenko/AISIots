using System.Security.Claims;
using AISIots.ViewModels;

namespace AISIots.Interfaces;

public interface IAuthService
{
    Task<bool> ConfirmLoginPassword(string login, string password);
    Task AddUserToDb(string login, string password);
    Task<bool> NeedToFirstRegister();
    Task<LoginModel> ProcessLogin(string? login, string? password, string? confirmPassword);
    Task<string> GetUserRole(string login);
    ClaimsPrincipal CreatePrincipal(string login);
}
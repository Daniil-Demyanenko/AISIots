using AISIots.Models;
using AISIots.ViewModels;

namespace AISIots.Interfaces;

public interface IAuthService
{
    bool ConfirmLoginPassword(string login, string password);
    void AddUserToDb(string login, string password);
    bool NeedToFirstRegister();
    LoginModel ProcessLogin(string? login, string? password, string? confirmPassword);
}

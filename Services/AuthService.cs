using System.Security.Cryptography;
using System.Text;
using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.Models;
using AISIots.ViewModels;
using AISIots.Models.DbTables;

namespace AISIots.Services;

public class AuthService(SqliteContext db) : IAuthService
{
    private const int MinPassLen = 6;

    public bool ConfirmLoginPassword(string login, string password)
    {
        using SHA256 sha = SHA256.Create();
        var pass = sha.ComputeHash(Encoding.ASCII.GetBytes(password));
        return db.Users.Any(x => x.Login == login && x.Password == pass);
    }

    public void AddUserToDb(string login, string password)
    {
        using SHA256 sha = SHA256.Create();
        var pass = sha.ComputeHash(Encoding.ASCII.GetBytes(password));
        var user = new User() { Login = login, Password = pass };
        db.Users.Add(user);
        db.SaveChanges();
    }

    public bool NeedToFirstRegister() => !db.Users.Any();

    public LoginModel ProcessLogin(string? login, string? password, string? confirmPassword)
    {
        var model = new LoginModel { NeedToRegisterFirstUser = NeedToFirstRegister() };

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            return model;

        model.SmallPass = password.Length < MinPassLen;
        if (model.SmallPass) return model;

        if (model.NeedToRegisterFirstUser)
        {
            if (password != confirmPassword)
            {
                model.CheckPassConfirmError = true;
                return model;
            }
            AddUserToDb(login, password);
            model.SuccessAuth = true;
            model.NeedToRegisterFirstUser = false;
            return model;
        }

        model.SuccessAuth = ConfirmLoginPassword(login, password);
        return model;
    }
}

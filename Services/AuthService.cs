using System.Security.Cryptography;
using System.Text;
using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.ViewModels;
using AISIots.Models.DbTables;
using System.Security.Claims;

namespace AISIots.Services;

public class AuthService(IDbRepository repository) : IAuthService
{
    private const int MinPassLen = 6;

    public async Task<bool> ConfirmLoginPassword(string login, string password)
    {
        using SHA256 sha = SHA256.Create();
        var pass = sha.ComputeHash(Encoding.ASCII.GetBytes(password));
        var user = await repository.GetUserByLoginAsync(login);
        return user != null && user.Password.SequenceEqual(pass);
    }

    public async Task AddUserToDb(string login, string password)
    {
        using SHA256 sha = SHA256.Create();
        var pass = sha.ComputeHash(Encoding.ASCII.GetBytes(password));
        var isFirstUser = await repository.AnyUsersAsync();
        var user = new User()
        {
            Login = login,
            Password = pass,
            RoleId = !isFirstUser ? 1 : 3 // 1 = MainAdmin, 3 = User
        };
        await repository.AddUserAsync(user);
    }

    public async Task<bool> NeedToFirstRegister() => !await repository.AnyUsersAsync();

    public async Task<LoginModel> ProcessLogin(string? login, string? password, string? confirmPassword)
    {
        var needRegister = await NeedToFirstRegister();
        var model = new LoginModel { NeedToRegisterFirstUser = needRegister };

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

            await AddUserToDb(login, password);
            model.SuccessAuth = true;
            model.NeedToRegisterFirstUser = false;
            return model;
        }

        model.SuccessAuth = await ConfirmLoginPassword(login, password);
        return model;
    }

    public async Task<string> GetUserRole(string login)
    {
        var user = await repository.GetUserByLoginAsync(login);
        return user?.Role?.Name ?? "User";
    }

    public ClaimsPrincipal CreatePrincipal(string login)
    {
        var user = repository.GetUserByLoginAsync(login).Result;
        var role = user?.Role?.Name ?? "User";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, login),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "login");
        return new ClaimsPrincipal(identity);
    }
}
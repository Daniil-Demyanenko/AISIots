using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AISIots.DAL;
using AISIots.Models.DbTables;
using Microsoft.IdentityModel.Tokens;

namespace AISIots.Services;

public static class Authentication
{
    public static bool ConfirmLoginPassword(string login, string password, SqliteContext db)
    {
        using SHA256 sha = SHA256.Create();
        var pass = sha.ComputeHash(Encoding.ASCII.GetBytes(password));

        return db.Users.Any(x => x.Login == login && x.Password == pass);
    }

    public static void AddUserToDb(string login, string password, SqliteContext db)
    {
        using SHA256 sha = SHA256.Create();
        var pass = sha.ComputeHash(Encoding.ASCII.GetBytes(password));

        var user = new User() { Login = login, Password = pass };
        db.Users.Add(user);
        db.SaveChanges();
    }

    public static bool NeedToFirstRegister(SqliteContext db)
        => !db.Users.Any();
}
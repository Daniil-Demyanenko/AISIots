using AISIots.DAL;
using AISIots.Services;

namespace AISIots.Models;

public class LoginModel
{
    public bool NeedToRegisterFirstUser;
    public bool CheckPassConfirmError = false;
    public bool SmallPass = false;
    public bool SuccessAuth = false;
    private const int MinPassLen = 6;

    private SqliteContext _db;

    #region

    // Если ты читаешь это, прости за говнокод... ночь и мало времени

    #endregion

    public LoginModel(SqliteContext db, string? login, string? password, string? confirmPassword = null)
    {
        _db = db;

        NeedToRegisterFirstUser = Authentication.NeedToFirstRegister(db);

        if (!ValidatePassword(login, password, confirmPassword))
            return;

        SuccessAuth = TryАuthenticate(login!, password!);
    }

    private bool ValidatePassword(string? login, string? password, string? confirmPassword)
    {
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)) return false;

        SmallPass = password.Length < MinPassLen;
        if (SmallPass) return false;

        if (NeedToRegisterFirstUser)
        {
            if (password != confirmPassword)
            {
                CheckPassConfirmError = true;
                return false;
            }

            Authentication.AddUserToDb(login, password, _db);
            SuccessAuth = true;
            NeedToRegisterFirstUser = false;
            return true;
        }

        return true;
    }

    private bool TryАuthenticate(string login, string password)
    {
        var userExist = Authentication.ConfirmLoginPassword(login, password, _db);

        return userExist;
    }
}
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

    #region

    // Если ты читаешь это, прости за говнокод... ночь и мало времени

    #endregion

    public LoginModel(SqliteContext db, string? login, string? password, string? confirmPassword = null)
    {
        NeedToRegisterFirstUser = Authentication.NeedToFirstRegister(db);

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)) return;
        
        SmallPass = password.Length < MinPassLen;

        if (NeedToRegisterFirstUser)
        {
            if (password != confirmPassword)
            {
                CheckPassConfirmError = true;
                return;
            }

            if (!SmallPass)
            {
                SuccessAuth = true;
                Authentication.AddUserToDb(login, password, db);
                return;
            }
        }

        var userExist = Authentication.ConfirmLoginPassword(login, password, db);
        if (userExist)
        {
            SuccessAuth = true;
            if(NeedToRegisterFirstUser) Authentication.AddUserToDb(login, password, db);
        }
    }
}
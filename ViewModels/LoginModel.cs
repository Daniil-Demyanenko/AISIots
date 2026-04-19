namespace AISIots.ViewModels;

public class LoginModel
{
    public bool NeedToRegisterFirstUser { get; set; }
    public bool CheckPassConfirmError { get; set; } = false;
    public bool SmallPass { get; set; } = false;
    public bool SuccessAuth { get; set; } = false;
}

namespace AISIots.ViewModels;

public class UserViewModel
{
    public string Login { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool RoleIdEditable { get; set; }
}

public class UserListModel
{
    public List<UserViewModel> Users { get; set; } = new();
}
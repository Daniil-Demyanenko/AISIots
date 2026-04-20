using AISIots.Models.DbTables;

namespace AISIots.ViewModels;

public class SettingsModel
{
    public bool IsAdmin { get; set; }
    public string CurrentUserLogin { get; set; } = string.Empty;
    public string CurrentUserRole { get; set; } = string.Empty;
    public string? Message { get; set; }
    public List<UserViewModel> Users { get; set; } = new();
    public List<ActionLog> Logs { get; set; } = new();
    public List<ActionLog> LogsPage { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}
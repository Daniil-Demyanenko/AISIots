using AISIots.Models.DbTables;

namespace AISIots.Interfaces;

public interface IActionLogService
{
    Task LogActionAsync(string userLogin, string action, string entityType, string targetName);
    Task<List<ActionLog>> GetLogsAsync();
    Task<List<ActionLog>> GetLogsByUserAsync(string userLogin);
}
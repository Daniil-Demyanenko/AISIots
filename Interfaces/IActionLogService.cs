using AISIots.Models.DbTables;

namespace AISIots.Interfaces;

public interface IActionLogService
{
    Task LogActionAsync(string userLogin, string action, string entityType, string targetName);
    Task<List<ActionLog>> GetLogsAsync(int page, int pageSize);
    Task<List<ActionLog>> GetLogsByUserAsync(string userLogin, int page, int pageSize);
    Task<int> GetLogsCountAsync();
    Task<int> GetLogsCountByUserAsync(string userLogin);
}
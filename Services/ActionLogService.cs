using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.Models.DbTables;

namespace AISIots.Services;

public class ActionLogService(IDbRepository repository) : IActionLogService
{
    public async Task LogActionAsync(string userLogin, string action, string entityType, string targetName)
    {
        var log = new ActionLog
        {
            UserLogin = userLogin,
            Action = action,
            EntityType = entityType,
            TargetName = targetName,
            Timestamp = DateTime.Now
        };
        await repository.AddActionLogAsync(log);
    }

    public async Task<List<ActionLog>> GetLogsAsync(int page, int pageSize)
    {
        var logs = await repository.GetActionLogsAsync();
        return logs
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<List<ActionLog>> GetLogsByUserAsync(string userLogin, int page, int pageSize)
    {
        var logs = await repository.GetActionLogsAsync();
        return logs
            .Where(l => l.UserLogin == userLogin)
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<int> GetLogsCountAsync()
    {
        var logs = await repository.GetActionLogsAsync();
        return logs.Count();
    }

    public async Task<int> GetLogsCountByUserAsync(string userLogin)
    {
        var logs = await repository.GetActionLogsAsync();
        return logs.Where(l => l.UserLogin == userLogin).Count();
    }
}
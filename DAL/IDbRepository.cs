using AISIots.Models.DbTables;
using Microsoft.EntityFrameworkCore.Storage;

namespace AISIots.DAL;

public interface IDbRepository
{
    // Plans
    Task<IEnumerable<Plan>> GetPlansAsync();
    Task<Plan?> GetPlanByIdAsync(int id);
    Task AddPlansAsync(IEnumerable<Plan> plans);
    Task<bool> IsContainLogicalSamePlanAsync(Plan plan);
    Task DeletePlanAsync(int id);

    // Rpds
    Task<IEnumerable<Rpd>> GetRpdsAsync();
    Task<Rpd?> GetRpdByIdAsync(int? id);
    Task AddRpdAsync(Rpd rpd);
    Task AddRpdsAsync(IEnumerable<Rpd> rpds);
    Task UpdateRpdAsync(Rpd rpd);
    Task DeleteRpdAsync(int? id);
    Task<bool> IsContainRpdWithSameTitleDifferentIdAsync(string title, int id);
    Task<bool> IsContainRpdWithTitleAsync(string title);
    Task<IOrderedEnumerable<string>> GetMissingRpds();

    // Users & Roles
    Task<IEnumerable<User>> GetUsersAsync();
    Task<User?> GetUserByLoginAsync(string login);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(string login);
    Task<bool> AnyUsersAsync();

    // ActionLogs
    Task AddActionLogAsync(ActionLog log);
    Task<IEnumerable<ActionLog>> GetActionLogsAsync();

    // Transactions and Save
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();
    void SaveChanges();
}
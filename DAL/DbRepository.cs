using AISIots.Models.DbTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AISIots.DAL;

public class DbRepository(SqliteContext db) : IDbRepository
{
    public async Task<IEnumerable<Plan>> GetPlansAsync() =>
        await db.Plans.Include(p => p.PlanBlocks).ThenInclude(pb => pb.DisciplineSections).ThenInclude(bs => bs.ShortRpds).ToListAsync();

    public async Task<Plan?> GetPlanByIdAsync(int id)
    {
        return await db.Plans
            .Include(p => p.PlanBlocks)
            .ThenInclude(pb => pb.DisciplineSections)
            .ThenInclude(bs => bs.ShortRpds)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddPlansAsync(IEnumerable<Plan> plans)
    {
        db.Plans.AddRange(plans);
        await db.SaveChangesAsync();
    }

    public async Task<bool> IsContainLogicalSamePlanAsync(Plan plan)
        => await db.Plans.IgnoreQueryFilters().AsAsyncEnumerable().AnyAsync(x =>
            x.Profile.Equals(plan.Profile, StringComparison.CurrentCultureIgnoreCase) &&
            x.GroupYear == plan.GroupYear &&
            x.Level.Equals(plan.Level, StringComparison.CurrentCultureIgnoreCase) &&
            (x.LearningForm ?? "").Equals(plan.LearningForm ?? "", StringComparison.CurrentCultureIgnoreCase));

    public async Task DeletePlanAsync(int id)
    {
        var plan = await db.Plans.FindAsync(id);
        if (plan != null)
        {
            plan.IsDeleted = true;
            db.Plans.Update(plan);
            await db.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Rpd>> GetRpdsAsync() => await db.Rpds.ToListAsync();

    public async Task<Rpd?> GetRpdByIdAsync(int? id) => await db.Rpds.FindAsync(id);

    public async Task AddRpdAsync(Rpd rpd) => await db.Rpds.AddAsync(rpd);

    public async Task AddRpdsAsync(IEnumerable<Rpd> rpds)
    {
        db.Rpds.AddRange(rpds);
        await db.SaveChangesAsync();
    }

    public async Task UpdateRpdAsync(Rpd rpd)
    {
        var existing = db.Rpds.Local.FirstOrDefault(x => x.Id == rpd.Id);
        if (existing != null)
            db.Entry(existing).State = EntityState.Detached;

        db.Rpds.Update(rpd);
        await db.SaveChangesAsync();
    }

    public async Task DeleteRpdAsync(int? id)
    {
        var rpd = await db.Rpds.FindAsync(id);
        if (rpd != null)
        {
            rpd.IsDeleted = true;
            db.Rpds.Update(rpd);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsContainRpdWithSameTitleDifferentIdAsync(string title, int id)
        => await db.Rpds.AsAsyncEnumerable().AnyAsync(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) && x.Id != id);

    public async Task<bool> IsContainRpdWithTitleAsync(string title)
        => await db.Rpds.AsAsyncEnumerable().AnyAsync(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));

    public async Task<IOrderedEnumerable<string>> GetMissingRpds()
    {
        var existingTitles = await db.Rpds
            .Where(r => !r.IsDeleted)
            .Select(r => r.Title.Trim().ToLower())
            .ToHashSetAsync();

        var missing = await db.Plans
            .Include(p => p.PlanBlocks)
            .ThenInclude(pb => pb.DisciplineSections)
            .ThenInclude(bs => bs.ShortRpds)
            .SelectMany(p => p.PlanBlocks)
            .SelectMany(b => b.DisciplineSections)
            .SelectMany(s => s.ShortRpds)
            .Select(rpd => rpd.Discipline)
            .Where(d => !string.IsNullOrEmpty(d) && !existingTitles.Contains(d.Trim().ToLower()))
            .ToHashSetAsync();

        return missing.Order();
    }

    public async Task<IEnumerable<User>> GetUsersAsync() => await db.Users.Include(u => u.Role).ToListAsync();

    public async Task<User?> GetUserByLoginAsync(string login) => await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Login == login);

    public async Task AddUserAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(string login)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
        if (user != null)
            db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    public async Task<bool> AnyUsersAsync() => await db.Users.AnyAsync();

    public async Task AddActionLogAsync(ActionLog log)
    {
        db.ActionLogs.Add(log);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<ActionLog>> GetActionLogsAsync() => await db.ActionLogs.OrderByDescending(x => x.Timestamp).ToListAsync();

    public Task<IDbContextTransaction> BeginTransactionAsync() => db.Database.BeginTransactionAsync();

    public async Task SaveChangesAsync() => await db.SaveChangesAsync();

    public void SaveChanges() => db.SaveChanges();
}
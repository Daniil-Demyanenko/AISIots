using AISIots.Models;
using AISIots.Models.DbTables;
using Microsoft.EntityFrameworkCore;

namespace AISIots.DAL;

public class DbRepository(SqliteContext db) : IDbRepository
{
    public bool IsContainLogicalSamePlan(Plan plan)
        => db.Plans.AsEnumerable().Any(x => x.Profile.Equals(plan.Profile, StringComparison.CurrentCultureIgnoreCase) && x.GroupYear == plan.GroupYear &&
                             x.Level.Equals(plan.Level, StringComparison.CurrentCultureIgnoreCase) &&
                             x.LearningForm.Equals(plan.LearningForm, StringComparison.CurrentCultureIgnoreCase));

    public bool IsContainRpdWithTitle(string title)
        => db.Rpds.AsEnumerable().Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));

    public bool IsContainRpdWithSameTitleDifferentId(string title, int id)
        => db.Rpds.AsEnumerable().Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) && x.Id != id);

    public async Task<IOrderedEnumerable<string>> GetMissingRpds()
    {
        var missing = await db.Plans
            .Include(p => p.PlanBlocks)
            .ThenInclude(pb => pb.DisciplineSections)
            .ThenInclude(bs => bs.ShortRpds)
            .SelectMany(p => p.PlanBlocks)
            .SelectMany(b => b.DisciplineSections)
            .SelectMany(s => s.ShortRpds)
            .AsAsyncEnumerable()
            .Select(rpd => rpd.Discipline)
            .Where(IsContainRpdWithTitle)
            .ToHashSetAsync();
        return missing.Order();
    }
}

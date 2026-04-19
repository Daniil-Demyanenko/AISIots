using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.Models;
using AISIots.ViewModels;
using AISIots.Models.DbTables;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;

namespace AISIots.Services;

public class PlanService(SqliteContext db, IDbRepository repository) : IPlanService
{
    public SearchModel Search(string? searchString, bool isRpdSearch)
    {
        if (string.IsNullOrEmpty(searchString?.Trim()))
        {
            var result = db.Rpds
                .OrderByDescending(x => x.UpdateDateTime).Take(50)
                .Select(x => new SearchItem(x.Id, x.Title));
            return new SearchModel { Items = result, IsRpdSearch = true };
        }

        var select = isRpdSearch
            ? db.Rpds.Select(x => new SearchItem(x.Id, x.Title))
            : db.Plans.Select(x => new SearchItem(x.Id, $"{GetFirstPart(x.Code)} - {x.Profile} ({x.GroupYear})"));

        var items = select.AsEnumerable()
            .OrderByDescending(x => Fuzz.TokenSortRatio(searchString.ToLower(), x.Title.ToLower()))
            .Take(50);

        return new SearchModel { Items = items, IsRpdSearch = isRpdSearch };
    }

    public Plan? GetPlanById(int id)
    {
        return db.Plans
            .Include(p => p.PlanBlocks)
            .ThenInclude(pb => pb.DisciplineSections)
            .ThenInclude(bs => bs.ShortRpds)
            .FirstOrDefault(p => p.Id == id);
    }

    public async Task<string?> UpdateRpdAsync(Rpd rpd)
    {
        if (string.IsNullOrEmpty(rpd.Title?.Trim()))
            return "Поле обязательно для заполнения";

        if (repository.IsContainRpdWithSameTitleDifferentId(rpd.Title, rpd.Id))
            return "Такая РПД уже существует";

        rpd.UpdateDateTime = DateTime.Now;
        db.Rpds.Update(rpd);
        await db.SaveChangesAsync();
        
        return null;
    }

    public async Task<string?> CreateRpdAsync(Rpd rpd)
    {
        if (string.IsNullOrEmpty(rpd.Title?.Trim()))
            return "Поле обязательно для заполнения";

        if (await db.Rpds.AnyAsync(r => r.Title.ToLower() == rpd.Title.ToLower()))
            return "РПД с таким названием уже существует";

        rpd.UpdateDateTime = DateTime.Now;
        rpd.Id = 0;
        db.Rpds.Add(rpd);
        await db.SaveChangesAsync();
        
        return null;
    }

    public async Task DeleteRpdAsync(int? id)
    {
        var rpdToDelete = await db.Rpds.FindAsync(id);
        if (rpdToDelete is not null) 
        {
            db.Rpds.Remove(rpdToDelete);
            await db.SaveChangesAsync();
        }
    }

    public async Task<Rpd> FindOrCreateRpdByIdAsync(int? id)
    {
        var rpd = await db.Rpds.FindAsync(id) ?? (await db.Rpds.AddAsync(new Rpd())).Entity;
        return Normalize(rpd);
    }

    private Rpd Normalize(Rpd rpd)
    {
        for (int i = rpd.Zad.Count; i < RpdFieldCounts.Zad; i++) rpd.Zad.Add("");
        for (int i = rpd.Nlec.Count; i < RpdFieldCounts.Nlec; i++) rpd.Nlec.Add("");
        for (int i = rpd.Nlab.Count; i < RpdFieldCounts.Nlab; i++) rpd.Nlab.Add("");
        for (int i = rpd.Npract.Count; i < RpdFieldCounts.Npract; i++) rpd.Npract.Add("");
        for (int i = rpd.Nsr.Count; i < RpdFieldCounts.Nsr; i++) rpd.Nsr.Add("");
        for (int i = rpd.OsnLitra.Count; i < RpdFieldCounts.OsnLitra; i++) rpd.OsnLitra.Add("");
        for (int i = rpd.DopLitra.Count; i < RpdFieldCounts.DopLitra; i++) rpd.DopLitra.Add("");
        for (int i = rpd.LecAnnotir.Count; i < RpdFieldCounts.LecAnnotir; i++) rpd.LecAnnotir.Add("");
        for (int i = rpd.KursRab.Count; i < RpdFieldCounts.KursRab; i++) rpd.KursRab.Add("");
        for (int i = rpd.Fos.Count; i < RpdFieldCounts.Fos; i++) rpd.Fos.Add("");
        for (int i = rpd.FosItog.Count; i < RpdFieldCounts.FosItog; i++) rpd.FosItog.Add("");

        return rpd;
    }

    private static string GetFirstPart(string? str) => str?.Split(' ')[0] ?? "";
}

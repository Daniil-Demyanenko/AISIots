using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.ViewModels;
using AISIots.Models;
using AISIots.Models.DbTables;
using FuzzySharp;

namespace AISIots.Services;

public class PlanService(IDbRepository repository) : IPlanService
{
    public async Task<SearchModel> Search(string? searchString, bool isRpdSearch)
    {
        if (string.IsNullOrEmpty(searchString?.Trim()))
        {
            if (isRpdSearch)
            {
                var result = (await repository.GetRpdsAsync())
                    .OrderByDescending(x => x.UpdateDateTime).Take(50)
                    .Select(x => new SearchItem(x.Id, x.Title)).ToList();
                return new SearchModel { Items = result, IsRpdSearch = true };
            }
            else
            {
                var result = (await repository.GetPlansAsync())
                    .OrderByDescending(x => x.GroupYear).Take(50)
                    .Select(x => new SearchItem(x.Id, $"{GetFirstPart(x.Code)} - {x.Profile} ({x.GroupYear}, {x.LearningForm}, {x.Level})")).ToList();
                return new SearchModel { Items = result, IsRpdSearch = false };
            }
        }

        var rpds = await repository.GetRpdsAsync();
        var plans = await repository.GetPlansAsync();

        var select = isRpdSearch
            ? rpds.Select(x => new SearchItem(x.Id, x.Title))
            : plans.Select(x => new SearchItem(x.Id, $"{GetFirstPart(x.Code)} - {x.Profile} ({x.GroupYear}, {x.LearningForm}, {x.Level})"));

        var items = select.OrderByDescending(x => Fuzz.TokenSortRatio(searchString.ToLower(), x.Title.ToLower()))
            .Take(50).ToList();

        return new SearchModel { Items = items, IsRpdSearch = isRpdSearch };
    }

    public async Task<Plan?> GetPlanByIdAsync(int id)
    {
        return await repository.GetPlanByIdAsync(id);
    }

    public async Task<Plan> GetPlanByIdOrThrowAsync(int id)
    {
        var plan = await repository.GetPlanByIdAsync(id);
        if (plan == null)
            throw new KeyNotFoundException($"План с ID {id} не найден");
        return plan;
    }

    public async Task<string?> UpdateRpdAsync(Rpd rpd)
    {
        if (string.IsNullOrEmpty(rpd.Title?.Trim()))
            return "Поле обязательно для заполнения";

        if (await repository.IsContainRpdWithSameTitleDifferentIdAsync(rpd.Title, rpd.Id))
            return "Такая РПД уже существует";

        rpd.UpdateDateTime = DateTime.Now;
        await repository.UpdateRpdAsync(rpd);

        return null;
    }

    public async Task<string?> CreateRpdAsync(Rpd rpd)
    {
        if (string.IsNullOrEmpty(rpd.Title?.Trim()))
            return "Поле обязательно для заполнения";

        if (await repository.IsContainRpdWithTitleAsync(rpd.Title))
            return "РПД с таким названием уже существует";

        rpd.UpdateDateTime = DateTime.Now;
        rpd.Id = 0;
        await repository.AddRpdAsync(rpd);
        await repository.SaveChangesAsync();

        return null;
    }

    public async Task DeleteRpdAsync(int? id)
    {
        await repository.DeleteRpdAsync(id);
        await repository.SaveChangesAsync();
    }

    public async Task DeletePlanAsync(int id)
    {
        await repository.DeletePlanAsync(id);
        await repository.SaveChangesAsync();
    }

    public async Task<Rpd> FindOrCreateRpdByIdAsync(int? id)
    {
        var rpd = await repository.GetRpdByIdAsync(id);
        if (rpd != null) return Normalize(rpd);
        
        rpd = new Rpd();
        await repository.AddRpdAsync(rpd);

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
using AISIots.DAL;
using AISIots.Models;
using AISIots.Models.DbTables;
using AISIots.Services;
using Microsoft.EntityFrameworkCore;

namespace AISIots.DAL;

public class DbRepository(SqliteContext db)
{
    public async Task<Rpd> FindOrCreateRpdById(int? id)
    {
        var rpd = await db.Rpds.FindAsync(id) ?? (await db.Rpds.AddAsync(new Rpd())).Entity;

        return Normalize(rpd);
    }

    public bool IsContainLogicalSamePlan(Plan plan)
        => db.Plans.AsEnumerable().Any(x => x.Profile.Equals(plan.Profile, StringComparison.CurrentCultureIgnoreCase) && x.GroupYear == plan.GroupYear &&
                             x.Level.Equals(plan.Level, StringComparison.CurrentCultureIgnoreCase) &&
                             x.LearningForm.Equals(plan.LearningForm, StringComparison.CurrentCultureIgnoreCase));

    public bool IsContainRpdWithTitle(string title)
    //AsEnumeralable, поскольку SQLite не умеет сравнивать кириллицу без учёта регистра
        => db.Rpds.AsEnumerable().Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));

    public bool IsContainRpdWithSameTitleDifferentId(string title, int id)
        => db.Rpds.AsEnumerable().Any(x => x.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase) && x.Id != id);

    /// <summary>
    /// Создаёт правильное количество полей, важно для интеграции с внешней системой
    /// </summary>
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

    /// <summary>
    /// Возвращает список РПД, которые упомянуты в планах, отсутствуют в БД
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Загрузить файлы в БД, автоматически определяет тип
    /// </summary>
    public async Task<(bool status, IEnumerable<string> problemFiles, IEnumerable<string> successFiles)>
        TryParseFilesFromDirectoryToDb(string pathToDir) // TODO: разбить на методы
    {
        List<string> problemFiles = new();
        List<string> successFiles = new();
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var fileWithTypes = Directory.GetFiles(pathToDir).Select(p =>
                (info: ExcelPatternMatcher.GetTemplateType(p), path: p));

            List<Rpd> rpds = new();
            List<Plan> plans = new();
            foreach (var f in fileWithTypes)
            {
                try
                {
                    if (f.info.Type == ExcelFileType.Rpd)
                    {
                        using var parser = new RpdParser(f.info, f.path);
                        var rpd = parser.Parse();
                        if (IsContainRpdWithTitle(rpd.Title))
                            problemFiles.Add($"{Path.GetFileNameWithoutExtension(f.path)} - [дубликат]");
                        else
                        {
                            rpds.Add(rpd);
                            successFiles.Add(Path.GetFileNameWithoutExtension(f.path));
                        }
                    }

                    if (f.info.Type == ExcelFileType.Plan)
                    {
                        using var parser = new PlanParser(f.info, f.path);
                        var plan = parser.Parse();
                        if (IsContainLogicalSamePlan(plan))
                            problemFiles.Add($"{Path.GetFileNameWithoutExtension(f.path)} - [дубликат]");
                        else
                        {
                            plans.Add(plan);
                            successFiles.Add(Path.GetFileNameWithoutExtension(f.path));
                        }
                    }

                    if (f.info.Type == ExcelFileType.Undefined) problemFiles.Add($"{Path.GetFileNameWithoutExtension(f.path)} - [нераспознан тип файла]");
                }
                catch
                {
                    problemFiles.Add($"{Path.GetFileNameWithoutExtension(f.path)} - [не удалось получить информацию]");
                }
            }

            if (rpds.Count == 0 && plans.Count == 0) return (false, problemFiles, successFiles);

            await db.Rpds.AddRangeAsync(rpds);
            await db.Plans.AddRangeAsync(plans);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, problemFiles, successFiles);
        }

        return (true, problemFiles, successFiles);
    }
}
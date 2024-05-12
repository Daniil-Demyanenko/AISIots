using AISIots.DAL;
using AISIots.Models.DbTables;

namespace AISIots.Services;

public static class FilesDbUpdater
{
    public static async Task<bool> TryParseFilesFromDirectoryToDb(string pathToDir, SqliteContext db)
    {
        var fileWithTypes = Directory.GetFiles(pathToDir).Select(p =>
            (info : ExcelPatternMatcher.GetTemplateType(p), path : p));

        List<Rpd> rpds = new();
        List<Plan> plans = new();
        foreach (var f in fileWithTypes)
        {
            if (f.info.Type == ExcelFileType.Rpd)
            {
                using var parser = new RpdParser(f.info, f.path);
                rpds.Add(parser.Parse());
            }
            if (f.info.Type == ExcelFileType.Plan)
            {
                using var parser = new PlanParser(f.info, f.path);
                plans.Add(parser.Parse());
            }
        }

        if (rpds.Count == 0 && plans.Count == 0) return false;
        
        await db.RPD.AddRangeAsync(rpds);
        await db.Plans.AddRangeAsync(plans);
        
        return true;
    }
}
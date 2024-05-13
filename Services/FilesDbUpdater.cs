using AISIots.DAL;
using AISIots.Models.DbTables;

namespace AISIots.Services;

public static class FilesDbUpdater
{
    public static async Task<(bool status, IEnumerable<string> problemFiles, IEnumerable<string> successFiles)> TryParseFilesFromDirectoryToDb(string pathToDir,
        SqliteContext db)
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
                        if (db.Rpds?.Any(x => x.Title == rpd.Title) == true)
                            problemFiles.Add($"{Path.GetFileNameWithoutExtension(f.path)} -- дубликат");
                        else
                        {
                            rpds.Add(rpd);
                            successFiles.Add(Path.GetFileNameWithoutExtension(f.path));
                        }
                    }

                    if (f.info.Type == ExcelFileType.Plan) // TODO: Проверка на дубликаты
                    {
                        using var parser = new PlanParser(f.info, f.path);
                        plans.Add(parser.Parse());
                        successFiles.Add(Path.GetFileNameWithoutExtension(f.path));
                    }

                    if (f.info.Type == ExcelFileType.Undefined) problemFiles.Add($"{Path.GetFileNameWithoutExtension(f.path)} -- нераспознан тип файла");
                }
                catch (Exception e)
                {
                    problemFiles.Add($"{Path.GetFileNameWithoutExtension(f.path)} -- не удалось получить информацию");
                }
            }

            if (rpds.Count == 0 && plans.Count == 0) return (false, problemFiles, successFiles);

            await db.Rpds!.AddRangeAsync(rpds);
            await db.Plans!.AddRangeAsync(plans);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return (false, problemFiles, successFiles);
        }

        return (true, problemFiles, successFiles);
    }
}
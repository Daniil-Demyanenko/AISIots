using AISIots.DAL;
using AISIots.Models.DbTables;
using AISIots.Utils;

namespace AISIots.Models;

public class UploadExcelFilesModel
{
    public bool LoadSuccessful { get; private set; }
    public IEnumerable<(ExcelPatternMatchingResult Info, string Path)>? Files;

    private UploadExcelFilesModel(bool loadSuccessful, IEnumerable<(ExcelPatternMatchingResult Info, string Path)>? files = null)
    {
        LoadSuccessful = loadSuccessful;
        Files = files;
    }

    public static async Task<UploadExcelFilesModel> Create(List<IFormFile>? files, SqliteContext db)
    {
        if (files == null || files.Count == 0) return new UploadExcelFilesModel(loadSuccessful: false);

        var pathToDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache", DateTime.Now.ToString("dd-MM-yy-") + Guid.NewGuid().ToString().Split('-')[0]);
        Directory.CreateDirectory(pathToDir);

        foreach (var file in files)
        {
            if (file.Length == 0 && Path.GetExtension(file.FileName) == ".xlsx") continue;

            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(pathToDir, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        await TryParseFilesToDb(pathToDir, db);

        return new UploadExcelFilesModel(loadSuccessful: true);
    }

    private static async Task<bool> TryParseFilesToDb(string pathToDir, SqliteContext db)
    {
        var fileWithTypes = Directory.GetFiles(pathToDir).Select(path =>
            (info : ExcelPatternMatcher.GetTemplateType(path), path : path));

        List<RPD> rpds = new();
        List<Plan> plans = new();
        foreach (var f in fileWithTypes)
        {
            if (f.info.Type == ExcelFileType.RPD)
            {
                using var parser = new RPDParser(f.info, f.path);
                rpds.Add(parser.Parse());
            }
            if (f.info.Type == ExcelFileType.Plan)
            {
                using var parser = new PlanParser(f.info, f.path);
                plans.Add(parser.Parse());
            }
        }

        await db.RPD.AddRangeAsync(rpds);
        await db.Plans.AddRangeAsync(plans);
        return true;
    }
}
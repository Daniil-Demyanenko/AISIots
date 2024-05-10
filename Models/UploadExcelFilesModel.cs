using AISIots.DAL;
using AISIots.Models.DbTables;
using AISIots.Utils;

namespace AISIots.Models;

public class UploadExcelFilesModel
{
    public bool LoadSuccessful { get; private set; }
    private readonly string? PathToDir;
    public IEnumerable<(ExcelPatternMatchingResult Info, string Path)>? Files;
    private SqliteContext? _db;

    private UploadExcelFilesModel(bool loadSuccessful, string? pathToDir = null, SqliteContext? db = null)
    {
        LoadSuccessful = loadSuccessful;
        PathToDir = pathToDir;
        _db = db;

        if (!LoadSuccessful) return;
        
        Files = Directory.GetFiles(PathToDir!).Select(x =>
            (ExcelPatternMatcher.GetTemplateType(x), x));

        List<RPD> rpds = new();
        foreach (var f in Files.Where(x=> x.Info.Type == ExcelFileType.RPD))
        {
            using RPDParser parser = new(f.Info, f.Path);
            rpds.Add(parser.Parse());
        }
        
        AddToDb(rpds);
    }

    public static async Task<UploadExcelFilesModel> Create(List<IFormFile>? files, SqliteContext db)
    {
        if (files == null || files.Count == 0) return new UploadExcelFilesModel(loadSuccessful: false);

        var pathToDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache",DateTime.Now.ToString("dd-MM-yy-") + Guid.NewGuid().ToString().Split('-')[0]);
        Directory.CreateDirectory(pathToDir);

        foreach (var file in files)
        {
            if (file.Length == 0 && Path.GetExtension(file.FileName) == ".xlsx") continue;

            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(pathToDir, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        if (Directory.GetFiles(pathToDir).Length == 0)
        {
            Directory.Delete(pathToDir);
            return new UploadExcelFilesModel(loadSuccessful: false);
        }

        return new UploadExcelFilesModel(loadSuccessful: true, pathToDir, db);
    }

    private void AddToDb(IEnumerable<RPD> rpds)
    {
        _db.AddRange(rpds);
        _db.SaveChanges();
    }
}
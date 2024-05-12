using AISIots.DAL;
using AISIots.Services;

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

        await FilesDbUpdater.TryParseFilesFromDirectoryToDb(pathToDir, db);

        return new UploadExcelFilesModel(loadSuccessful: true);
    }

    
}
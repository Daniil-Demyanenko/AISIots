using AISIots.DAL;
using AISIots.Services;

namespace AISIots.Models;

public class UploadExcelFilesModel
{
    public bool LoadSuccessful { get; private set; }
    public IEnumerable<string> SuccessFiles;
    public IEnumerable<string> ProblemFiles;

    private UploadExcelFilesModel(bool loadSuccessful, IEnumerable<string> successFiles, IEnumerable<string> problemFiles)
    {
        LoadSuccessful = loadSuccessful;
        ProblemFiles = problemFiles;
        SuccessFiles = successFiles;
    }

    public static async Task<UploadExcelFilesModel> Create(List<IFormFile>? files, SqliteContext db)
    {
        if (files == null || files.Count == 0) return new UploadExcelFilesModel(loadSuccessful: false, [],[]);

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

        var (parseSuccess, problemFiles, successFiles) = await FilesDbLoader.TryParseFilesFromDirectoryToDb(pathToDir, db);
        if (!parseSuccess)
            return new UploadExcelFilesModel(loadSuccessful: false, successFiles, problemFiles);

        return new UploadExcelFilesModel(loadSuccessful: true, successFiles, problemFiles);
    }
}
using AISIots.Utils;

namespace AISIots.Models;

public class UploadExcelFilesModel
{
    public bool LoadSuccessful { get; private set; }
    public readonly string? PathToDir;
    public IEnumerable<(ExcelFileType Type, string Name)>? Files;

    private UploadExcelFilesModel(bool loadSuccessful, string? pathToDir = null)
    {
        LoadSuccessful = loadSuccessful;
        PathToDir = pathToDir;

        if (!LoadSuccessful) return;
        Files = Directory.GetFiles(PathToDir!).Select(x =>
            (ExcelPatternMatcher.GetTemplateType(x).Type, Path.GetFileName(x)));
    }

    public static async Task<UploadExcelFilesModel> Create(List<IFormFile>? files)
    {
        bool isSuccessful = false;
        if (files == null) return new UploadExcelFilesModel(isSuccessful);

        var pathToDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache", Guid.NewGuid().ToString().Split('-')[^1]);
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
            return new UploadExcelFilesModel(isSuccessful);
        }

        isSuccessful = true;
        return new UploadExcelFilesModel(isSuccessful, pathToDir);
    }
}
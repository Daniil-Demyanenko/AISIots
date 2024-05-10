namespace AISIots.Models;

public class ExcelFilesModel
{
    public bool LoadSuccessful { get; private set; }
    private string? _pathToDir;

    private ExcelFilesModel(bool loadSuccessful, string? pathToDir = null)
    {
        LoadSuccessful = loadSuccessful;
        _pathToDir = pathToDir;
    }

    public static async Task<ExcelFilesModel> Create(List<IFormFile>? files)
    {
        bool isSuccessful = false;
        if (files == null) return new ExcelFilesModel(isSuccessful);

        var pathToDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache", Guid.NewGuid().ToString().Split('-')[^1]);
        Directory.CreateDirectory(pathToDir);

        foreach (var file in files)
        {
            if (file.Length == 0 && Path.GetExtension(file.FileName) is not (".xls" or ".xlsx")) continue;

            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(pathToDir, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        if (Directory.GetFiles(pathToDir).Length == 0)
        {
            Directory.Delete(pathToDir);
            return new ExcelFilesModel(isSuccessful);
        }
        
        isSuccessful = true;
        return new ExcelFilesModel(isSuccessful, pathToDir);
    }

    public IEnumerable<string> GetFileNames()
    {
        return Directory.GetFiles(_pathToDir!).Select(Path.GetFileName)!;
    }
}
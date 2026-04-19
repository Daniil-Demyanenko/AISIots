using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.Models;
using AISIots.ViewModels;
using AISIots.Models.DbTables;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AISIots.Services;

public class FileProcessingService(SqliteContext db, IDbRepository repository, IParserFactory parserFactory) : IFileProcessingService
{
    public async Task<UploadExcelFilesModel> ProcessUploadedFilesAsync(List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) 
            return new UploadExcelFilesModel { LoadSuccessful = false };

        var pathToDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache", DateTime.Now.ToString("dd-MM-yy-") + Guid.NewGuid().ToString().Split('-')[0]);
        Directory.CreateDirectory(pathToDir);

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var fileName = Path.GetFileName(file.FileName);
            var path = Path.Combine(pathToDir, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        var (parseSuccess, problemFiles, successFiles) = await TryParseFilesFromDirectoryToDb(pathToDir);
        
        if (Directory.Exists(pathToDir))
        {
            Directory.Delete(pathToDir, true);
        }
        
        return new UploadExcelFilesModel 
        { 
            LoadSuccessful = parseSuccess, 
            SuccessFiles = successFiles, 
            ProblemFiles = problemFiles 
        };
    }

    private async Task<(bool status, IEnumerable<string> problemFiles, IEnumerable<string> successFiles)> TryParseFilesFromDirectoryToDb(string pathToDir)
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
                        using var parser = parserFactory.CreateRpdParser(f.info, f.path);
                        var rpd = parser.Parse();
                        if (repository.IsContainRpdWithTitle(rpd.Title))
                            problemFiles.Add($"{Path.GetFileNameWithoutExtension(f.path)} - [дубликат]");
                        else
                        {
                            rpds.Add(rpd);
                            successFiles.Add(Path.GetFileNameWithoutExtension(f.path));
                        }
                    }

                    if (f.info.Type == ExcelFileType.Plan)
                    {
                        using var parser = parserFactory.CreatePlanParser(f.info, f.path);
                        var plan = parser.Parse();
                        if (repository.IsContainLogicalSamePlan(plan))
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

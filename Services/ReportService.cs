using System.Text.Json;
using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.Models;
using AISIots.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AISIots.Services;

public class ReportService(SqliteContext db, IDbRepository repository) : IReportService
{
    public async Task<MissingReportModel> GetMissingRpdsReportAsync()
    {
        var missingRpds = await repository.GetMissingRpds();
        return new MissingReportModel { MissingRpds = missingRpds };
    }

    public ExportJsonModel GetExportJson()
    {
        var plans = db.Plans
            .Include(p => p.PlanBlocks)
            .ThenInclude(pb => pb.DisciplineSections)
            .ThenInclude(bs => bs.ShortRpds).AsEnumerable();
        var rpds = db.Rpds.AsEnumerable();

        dynamic toExport = new { Plans = plans, RPDs = rpds };

        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        return new ExportJsonModel { Json = JsonSerializer.Serialize(toExport, options) };
    }
}

using System.Text.Json;
using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.ViewModels;

namespace AISIots.Services;

public class ReportService(IDbRepository repository) : IReportService
{
    public async Task<MissingReportModel> GetMissingRpdsReportAsync()
    {
        var missingRpds = await repository.GetMissingRpds();
        return new MissingReportModel { MissingRpds = missingRpds };
    }

    public async Task<ExportJsonModel> GetExportJsonAsync()
    {
        var plans = await repository.GetPlansAsync();
        var rpds = await repository.GetRpdsAsync();

        dynamic toExport = new { Plans = plans, RPDs = rpds };

        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return new ExportJsonModel { Json = JsonSerializer.Serialize(toExport, options) };
    }
}
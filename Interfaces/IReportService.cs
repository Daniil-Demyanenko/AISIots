using AISIots.Models;
using AISIots.ViewModels;

namespace AISIots.Interfaces;

public interface IReportService
{
    Task<MissingReportModel> GetMissingRpdsReportAsync();
    ExportJsonModel GetExportJson();
}

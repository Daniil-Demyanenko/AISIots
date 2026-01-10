using AISIots.DAL;
using AISIots.Models.DbTables;
using AISIots.Services;
using Microsoft.EntityFrameworkCore;

namespace AISIots.Models;

public class MissingReportModel
{
    public IEnumerable<string> MissingRpds { get; init; }

    private MissingReportModel(IEnumerable<string> missingRpds)
    {
        MissingRpds = missingRpds;
    }

    public static async Task<MissingReportModel> Create(DbRepository repository)
    {
        var missingRpds = await repository.GetMissingRpds();
        return new MissingReportModel(missingRpds);
    }
}
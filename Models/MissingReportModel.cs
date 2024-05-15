using AISIots.DAL;
using AISIots.Services;
using Microsoft.EntityFrameworkCore;

namespace AISIots.Models;

public class MissingReportModel
{
    public IEnumerable<string> MissingRpds;

    public MissingReportModel(SqliteContext db)
    {
        MissingRpds = db.Plans
            .Include(p => p.PlanBlocks)
            .ThenInclude(pb => pb.BlockSections)
            .ThenInclude(bs => bs.ShortRpds)
            .SelectMany(p=>p.PlanBlocks)
            .SelectMany(b=>b.BlockSections)
            .SelectMany(s=>s.ShortRpds)
            .AsEnumerable()
            .Where(rpd=>!DbFinder.IsContainRpdWithTitle(rpd.Discipline, db))
            .Select(rpd=> $"{rpd.Index} - {rpd.Discipline}")
            .ToHashSet()
            .Order();
        }
}
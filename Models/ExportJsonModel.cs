using System.Text.Json;
using AISIots.DAL;
using Microsoft.EntityFrameworkCore;

namespace AISIots.Models;

public class ExportJsonModel
{
    public readonly string Json;
    
    public ExportJsonModel(SqliteContext db)
    {
        var plans = db.Plans
            .Include(p => p.PlanBlocks)
            .ThenInclude(pb => pb.BlockSections)
            .ThenInclude(bs => bs.ShortRpds).AsEnumerable();
        var rpds = db.Rpds.AsEnumerable();
        
        dynamic toExport = new { Plans = plans, RPDs = rpds };
        
        var options = new JsonSerializerOptions() { WriteIndented = true };
        Json = JsonSerializer.Serialize(toExport, options);
    }
}
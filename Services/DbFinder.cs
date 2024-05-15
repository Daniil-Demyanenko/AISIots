using AISIots.DAL;
using AISIots.Models;
using AISIots.Models.DbTables;

namespace AISIots.Services;

public static class DbFinder
{
    public static async Task<Rpd> FindOrCreateById(int? id, SqliteContext db)
    {
        var rpd = await db.Rpds.FindAsync(id);
        if (rpd is null)
        {
            rpd = (await db.Rpds.AddAsync(new Rpd())).Entity;
        }
    
        return Normalize(rpd);
    }

    public static bool IsContainRpdWithTitle(string title, SqliteContext db)
        => db.Rpds.Any(x => x.Title == title);

    public static bool IsContainSameTitleDifferentId(string title, int id, SqliteContext db)
        => db.Rpds.Any(x => x.Title == title && x.Id != id);


    private static Rpd Normalize(Rpd rpd)
    {
        for (int i = rpd.Zad.Count; i < RpdFieldCounts.Zad; i++) rpd.Zad.Add("");
        for (int i = rpd.Nlec.Count; i < RpdFieldCounts.Nlec; i++) rpd.Nlec.Add("");
        for (int i = rpd.Nlab.Count; i < RpdFieldCounts.Nlab; i++) rpd.Nlab.Add("");
        for (int i = rpd.Npract.Count; i < RpdFieldCounts.Npract; i++) rpd.Npract.Add("");
        for (int i = rpd.Nsr.Count; i < RpdFieldCounts.Nsr; i++) rpd.Nsr.Add("");
        for (int i = rpd.OsnLitra.Count; i < RpdFieldCounts.OsnLitra; i++) rpd.OsnLitra.Add("");
        for (int i = rpd.DopLitra.Count; i < RpdFieldCounts.DopLitra; i++) rpd.DopLitra.Add("");
        for (int i = rpd.LecAnnotir.Count; i < RpdFieldCounts.LecAnnotir; i++) rpd.LecAnnotir.Add("");
        for (int i = rpd.KursRab.Count; i < RpdFieldCounts.KursRab; i++) rpd.KursRab.Add("");
        for (int i = rpd.Fos.Count; i < RpdFieldCounts.Fos; i++) rpd.Fos.Add("");
        for (int i = rpd.FosItog.Count; i < RpdFieldCounts.FosItog; i++) rpd.FosItog.Add("");

        return rpd;
    }
}
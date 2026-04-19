using AISIots.Interfaces;
using AISIots.Models.DbTables;

namespace AISIots.Services;

public class ParserFactory : IParserFactory
{
    public IExcelParser<Rpd> CreateRpdParser(ExcelPatternMatchingResult info, string path)
    {
        return new RpdParser(info, path);
    }

    public IExcelParser<Plan> CreatePlanParser(ExcelPatternMatchingResult info, string path)
    {
        return new PlanParser(info, path);
    }
}

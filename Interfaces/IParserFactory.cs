using AISIots.Models.DbTables;
using AISIots.Services;

namespace AISIots.Interfaces;

public interface IParserFactory
{
    IExcelParser<Rpd> CreateRpdParser(ExcelPatternMatchingResult info, string path);
    IExcelParser<Plan> CreatePlanParser(ExcelPatternMatchingResult info, string path);
}

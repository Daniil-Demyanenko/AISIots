using ClosedXML.Excel;

namespace AISIots.Services;

public record ExcelPatternMatchingResult(ExcelFileType Type, int WorksheetPosition);

public static class ExcelPatternMatcher
{
    public static ExcelPatternMatchingResult GetTemplateType(string path)
    {
        using var workbook = new XLWorkbook(path);

        var rpdPage = IndexOfPageWithRPD(workbook);
        if (rpdPage != -1) return new ExcelPatternMatchingResult(ExcelFileType.Rpd, rpdPage);

        var planPage = IndexOfPageWithPlan(workbook);
        if (planPage != -1) return new ExcelPatternMatchingResult(ExcelFileType.Plan, planPage);

        return new ExcelPatternMatchingResult(ExcelFileType.Undefined, -1);
    }

    public static (int PlanPage, int TitlePage) GetImportantPlanPages(string path)
    {
        using var wb = new XLWorkbook(path);

        return (IndexOfPageWithPlan(wb), IndexOfPageWithPlanTitle(wb));
    }

    private static int IndexOfPageWithRPD(XLWorkbook workbook)
    {
        foreach (var worksheet in workbook.Worksheets)
        {
            var cellA1 = worksheet.Cell("A1").Value.ToString().Trim();
            var cellB1 = worksheet.Cell("B1").Value.ToString().Trim();
            var cellC1 = worksheet.Cell("C1").Value.ToString().Trim();

            if (cellA1 == "1" && cellB1 == "2" && cellC1 == string.Empty)
                return worksheet.Position;
        }

        return -1;
    }

    private static int IndexOfPageWithPlan(XLWorkbook workbook)
    {
        foreach (var worksheet in workbook.Worksheets)
        {
            for (int i = 1; i < 10; i++)
            {
                if (worksheet.RangeUsed().LastColumn().ColumnNumber() < 50) continue;

                var cell1 = worksheet.Cell(i, 1).Value.ToString().Trim();
                var cell2 = worksheet.Cell(i + 1, 1).Value.ToString().Trim();
                var cell3 = worksheet.Cell(i + 2, 1).Value.ToString().Trim();

                if (cell1.Contains("Блок") && cell2.Length > 1 && cell3.Length > 0 && cell3[0] == '+')
                    return worksheet.Position;
            }
        }

        return -1;
    }

    private static int IndexOfPageWithPlanTitle(XLWorkbook workbook)
    {
        foreach (var worksheet in workbook.Worksheets)
        {
            var visibleRows = new List<int>();
            for (int i = 1; i <= 100; i++)
            {
                var cell = worksheet.Cell(i, 1);
                if (cell.WorksheetColumn().IsHidden || cell.WorksheetRow().IsHidden) continue;
                visibleRows.Add(i);
            }

            for (int i = 0; i < visibleRows.Count - 5; i++)
            {
                var signs = new bool[4];

                signs[0] = worksheet.Cell(visibleRows[i], 1).Value.ToString().Contains("Профиль");
                signs[1] = worksheet.Cell(visibleRows[i + 1], 1).Value.ToString().Contains("Кафедра");
                signs[2] = worksheet.Cell(visibleRows[i + 2], 1).Value.ToString().Contains("Институт");
                signs[3] = worksheet.Cell(visibleRows[i + 5], 1).Value.ToString().Contains("Программа подготовки");

                if (signs.All(x => x))
                    return worksheet.Position;
            }
        }

        return -1;
    }
}
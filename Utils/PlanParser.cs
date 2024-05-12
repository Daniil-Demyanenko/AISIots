using System.Text.RegularExpressions;
using AISIots.Models;
using AISIots.Models.DbTables;
using ClosedXML.Excel;

namespace AISIots.Utils;

public class PlanParser : IDisposable
{
    private readonly string _path;
    private readonly XLWorkbook _wb;
    private readonly IXLWorksheet _wsTitle;
    private readonly IXLWorksheet _wsPlan;

    public PlanParser(ExcelPatternMatchingResult info, string path)
    {
        if (info.Type != ExcelFileType.Plan) throw new Exception("this is not a Plan");
        _wb = new XLWorkbook(path);
        _path = path;

        var (planPage, titlePage) = ExcelPatternMatcher.GetImportantPlanPages(path);
        if (planPage == -1 || titlePage == -1) throw new Exception("Important page not found");
        _wsTitle = _wb.Worksheet(titlePage);
        _wsPlan = _wb.Worksheet(planPage);
    }

    public Plan Parse()
    {
        var plan = GetPlanWithOnlyTitle();
        plan.GroupYear = GetGroupYear();
        plan.PlanBlocks = GetPlanBlocks();
        
        return plan;
    }

    private List<PlanBlock> GetPlanBlocks()
    {
        List<PlanBlock> planBloks = new();

        int lastRow = _wsPlan.LastRowUsed().RowNumber();
        for (int blockRow = 1; blockRow < lastRow; blockRow++) // создание основных блоков
        {
            var cell = _wsPlan.Cell(blockRow, 1);
            var cellValue = cell.Value.ToString().Trim();

            if (!cellValue.Contains("Блок ") || !cell.IsMerged()) continue;

            planBloks.Add(new PlanBlock(cellValue));

            for (int sectionRow = blockRow + 1; sectionRow < lastRow; sectionRow++) // создание секций в блоке
            {
                cell = _wsPlan.Cell(sectionRow, 1);
                cellValue = cell.Value.ToString().Trim();
                if (!cell.IsMerged()) continue; // в ячейке хранится дисциплина
                if (cellValue.Contains("Блок ")) break; // начался следующий блок

                planBloks.Last().SupBlocks.Add(new PlanBlock(cellValue));

                for (int itemRow = sectionRow + 1; itemRow < lastRow; itemRow++) // создание записей в каждой секции
                {
                    cell = _wsPlan.Cell(sectionRow, 1);
                    if (cell.IsMerged()) break;

                    var index = _wsPlan.Cell(sectionRow, 2).Value.ToString().Trim();
                    var discipline = _wsPlan.Cell(sectionRow, 3).Value.ToString().Trim();
                    var title = index + " " + discipline;
                    
                    planBloks.Last().SupBlocks.Last().SupBlocks.Add(new PlanBlock(title));
                }
            }
        }

        return planBloks;
    }

    private int GetGroupYear()
    {
        var (foundRow, foundCol) = (0, 0);
        int lastRow = _wsTitle.LastRowUsed().RowNumber();
        int lastCol = _wsTitle.LastColumnUsed().ColumnNumber();

        for (int row = 1; row <= lastRow; row++)
        {
            for (int col = 1; col <= lastCol; col++)
            {
                if (_wsTitle.Cell(row, col).Value.ToString().Contains("Год начала подготовки"))
                    (foundRow, foundCol) = (row, col);
            }
        }

        for (int col = foundCol + 1; col < lastCol; col++)
        {
            int result;
            var cell = _wsTitle.Cell(foundRow, col).Value.ToString();
            if (int.TryParse(cell, out result)) return result;
        }
        
        return -1;
    }

    private Plan GetPlanWithOnlyTitle()
    {
        Plan result = new Plan();

        var visibleRows = new List<int>();
        int lastRow = _wsTitle.LastRowUsed().RowNumber();
        for (int i = 1; i <= lastRow; i++)
        {
            var cell = _wsTitle.Cell(i, 1);
            if (cell.WorksheetColumn().IsHidden || cell.WorksheetRow().IsHidden) continue;
            visibleRows.Add(i);
        }

        foreach (var row in visibleRows)
        {
            var key = _wsTitle.Cell(row, 1).Value.ToString().Trim();
            var value = _wsTitle.Cell(row, 2).Value.ToString().Trim();

            if (key.Contains("Профиль")) result.Profile = value;
            if (key.Contains("Кафедра")) result.Kafedra = value;
            if (key.Contains("Институт")) result.Institute = value;
            if (key.Contains("Программа подготовки")) result.Level = key.Split(' ')[^1];
            if (key.Contains("Форма обучения")) result.LearningForm = key.Split(' ')[^1];
            if (key.Contains("Срок получения образования")) result.LearningDuration = key.Split(':')[^1].Trim();
        }

        // Поиск кода направления обучения
        string pattern = @"^\s*\d{1,2}\.\d{1,2}\.\d{1,2}\s+[А-Яа-яA-Za-z\s]+$";
        Regex regex = new Regex(pattern);
        int lastCol = _wsTitle.LastColumnUsed().ColumnNumber();
        foreach (var row in visibleRows)
        {
            for (int col = 1; col <= lastCol; col++)
            {
                var current = _wsTitle.Cell(row, col).Value.ToString().Trim();
                if (regex.IsMatch(current))
                {
                    result.Code = current;
                    return result;
                }
            }
        }

        return result;
    }

    public void Dispose()
    {
        _wb?.Dispose();
    }
}
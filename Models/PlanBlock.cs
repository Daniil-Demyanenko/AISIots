using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace AISIots.Models;

public class PlanBlock(string title)
{
    public string Title { get; set; } = title;
    public List<PlanBlock> SupBlocks { get; set; } = new();
}
using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace AISIots.Models.DbTables;

public class PlanBlock(string title)
{
    public string Title { get; set; } = title;
    public List<BlockSection> BlockSections { get; set; } = new();
}
public class BlockSection(string title)
{
    public string Title { get; set; } = title;
    public List<ShortRpd> ShortRpds { get; set; } = new();
    
}

public class ShortRpd(string discipline, string index)
{
    public string Discipline { get; set; } = discipline;
    public string Index { get; set; } = index;
}
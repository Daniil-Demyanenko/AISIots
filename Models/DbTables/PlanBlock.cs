using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class PlanBlock(string title)
{
    [Key] public int Id { get; set; }
    public string Title { get; set; } = title;
    public List<BlockSection> BlockSections { get; set; } = new();
}

public class BlockSection(string title)
{
    [Key] public int Id { get; set; }
    public int PlanBlockId { get; set; }
    public string Title { get; set; } = title;
    public List<ShortRpd> ShortRpds { get; set; } = new();
}

public class ShortRpd(string discipline, string index)
{
    [Key] public int Id { get; set; }
    public int BlockSectionId { get; set; }
    public string Discipline { get; set; } = discipline;
    public string Index { get; set; } = index;
}
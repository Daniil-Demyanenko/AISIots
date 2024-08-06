using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class DisciplineBlock(string title)
{
    [Key] public int Id { get; set; }
    public string Title { get; set; } = title;
    public List<DisciplineSection> DisciplineSections { get; set; } = new();
}

public class DisciplineSection(string title)
{
    [Key] public int Id { get; set; }
    public int DisciplineBlockId { get; set; }
    public string Title { get; set; } = title;
    public List<ShortRpd> ShortRpds { get; set; } = new();
}

public class ShortRpd(string discipline, string index)
{
    [Key] public int Id { get; set; }
    public int DisciplineSectionId { get; set; }
    public string Discipline { get; set; } = discipline;
    public string Index { get; set; } = index;
}
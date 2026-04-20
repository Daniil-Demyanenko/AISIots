using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class Role
{
    [Key] public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
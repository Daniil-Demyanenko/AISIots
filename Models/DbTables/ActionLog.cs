using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class ActionLog
{
    [Key] public int Id { get; set; }
    public string UserLogin { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
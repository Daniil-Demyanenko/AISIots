using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class Plan
{
    [Key]
    public int Id { get; set; }
    public List<PlanBlock> PlanBlocks { get; set; }
    /// <summary>
    /// Год набора группы
    /// </summary>
    public int? GroupYear { get; set; }
    public DateTime UpdateDateTime { get; set; }
    public string Profile { get; set; }
    public string? Kafedra { get; set; }
    public string? Institute { get; set; }
    public string? LearningDuration { get; set; }
    public string? LearningForm { get; set; }
    public string? Code { get; set; }
    /// <summary>
    /// Магистратура, бакалавриат, аспирантура...
    /// </summary>
    public string Level { get; set; }

    public Plan()
    {
        UpdateDateTime = DateTime.Now;
    }
    
    public string GetFormatedDateTime()
    {
        return UpdateDateTime.ToString("dd.MM.yyyy-HH:mm");
    }
}
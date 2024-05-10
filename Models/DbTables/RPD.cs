using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class RPD
{
    [Key] public int Id { get; set; }
    public Plan? PlanId { get; set; }
    public string Title { get; set; }
    public string UpdateDateTime { get; set; }
    public string? DisciplineIndex { get; set; }
    public string? PrepodRegFull { get; set; }
    public string? PrepodRegFullShort { get; set; }
    public string? Razrab { get; set; }
    public string? RazrabShort { get; set; }
    public string? Tceli { get; set; }
    public string? Znat { get; set; }
    public string? Umet { get; set; }
    public string? Vladet { get; set; }
    public string? Osnna { get; set; }
    public string? Sldla { get; set; }
    public string? DopProgObesp { get; set; }
    public List<string> Zad { get; set; } = [];
    public List<string> Nlec { get; set; } = [];
    public List<string> Npract { get; set; } = [];
    public List<string> Nlab { get; set; } = [];
    public List<string> Nsr { get; set; } = [];
    public List<string> OsnLitra { get; set; } = [];
    public List<string> DopLitra { get; set; } = [];
    public List<string> LecAnnotir { get; set; } = [];
    public List<string> KursRab { get; set; } = [];
    public List<string> Fos { get; set; } = [];
    public List<string> FosItog { get; set; } = [];


    /// <summary>
    /// Устанавливает время последнего обновления программы дисциплины
    /// </summary>
    /// <param name="dt"></param>
    public void SetFormatedDateTime(DateTime dt)
    {
        UpdateDateTime = dt.ToString("dd/MM/yyyy-HH:mm");
    }
}


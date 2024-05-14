using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class Rpd
{
    [Key] public int Id { get; set; }
    public string Title { get; set; }
    public DateTime UpdateDateTime { get; set; }
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

    public Rpd()
    {
        UpdateDateTime = DateTime.Now;
    }

    public string GetFormatedDateTime()
    {
        return UpdateDateTime.ToString("dd.MM.yyyy-HH:mm");
    }
}
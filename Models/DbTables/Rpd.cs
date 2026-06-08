using System.ComponentModel.DataAnnotations;

namespace AISIots.Models.DbTables;

public class Rpd
{
    [Key] public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
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

    public string? Fak { get; set; }
    public string? FakShort { get; set; }
    public string? Kaf { get; set; }
    public string? KodKaf { get; set; }
    public string? KodSpec { get; set; }
    public string? Spec { get; set; }
    public string? Profil { get; set; }
    public string? Kvalif { get; set; }
    public string? FormaObuch { get; set; }
    public string? Sem { get; set; }
    public string? ZavKaf { get; set; }
    public string? DirFio { get; set; }
    public string? Standart { get; set; }
    public string? FosKomp { get; set; }
    public string? TekKontrol { get; set; }
    public string? FosFormaKontrol { get; set; }
    public string? UmkPreds { get; set; }
    public string? ProtUmk { get; set; }
    public string? ProtKaf { get; set; }
    public string? Chast { get; set; }
    public string? KodDisc { get; set; }
    public string? Itogo { get; set; }
    public string? Ze { get; set; }
    public string? Ksr { get; set; }
    public string? KsrZo { get; set; }
    public string? SamRab { get; set; }
    public string? SamRabZo { get; set; }
    public string? FormaKontrol11 { get; set; }
    public string? SemShort { get; set; }
    public string? Komp1n { get; set; }
    public string? Komp2n { get; set; }
    public string? Komp3n { get; set; }
    public string? Komp1N123 { get; set; }
    public string? Komp2N123 { get; set; }
    public string? Komp3N123 { get; set; }
    public string? Komp1N123Et { get; set; }
    public string? Komp2N123Et { get; set; }
    public string? Komp3N123Et { get; set; }

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
    public List<string> Komp { get; set; } = [];
    public List<int> Xl { get; set; } = [];
    public List<int> Xlzo { get; set; } = [];
    public List<int> Zl { get; set; } = [];
    public List<int> Zlzo { get; set; } = [];
    public List<int> Yl { get; set; } = [];
    public List<int> Ylzo { get; set; } = [];
    public List<int> Nnsr { get; set; } = [];
    public List<int> Wnsr { get; set; } = [];

    public bool IsDeleted { get; set; } = false;

    public Rpd()
    {
        UpdateDateTime = DateTime.Now;
    }

    public string GetFormatedDateTime()
    {
        return UpdateDateTime.ToString("dd.MM.yyyy-HH:mm");
    }
}
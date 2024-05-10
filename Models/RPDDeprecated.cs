using System.ComponentModel.DataAnnotations;

namespace AISIots.Models;

#region About Class
/* Такая страшная структура класса выбрана для простой конвертации в JSON
 * с правильно названными полями. Такая необходимость возникает из-за
 * уже существующего на кафердре кривого софта с захардкожеными полями,
 * с которым нужно интегрироваться.
 * Конечно, можно было не создавать 20 полей Fos, а создать отдельную таблицу, но потом
 * вручную формировать JSON с нужным количеством полей каждого типа.
 */
#endregion

public class RPDDeprecated
{
    [Key] public int Id { get; set; }
    public Plan PlanId { get; set; }
    public string Date { get; set; }
    public string PrepodRegFull { get; set; }
    public string PrepodRegFullShort { get; set; }
    public string Razrab { get; set; }
    public string RazrabShort { get; set; }
    public string Tceli { get; set; }
    public string Znat { get; set; }
    public string Umet { get; set; }
    public string Vladet { get; set; }
    public string Osnna { get; set; }
    public string Sldla { get; set; }
    public string DopProgObesp { get; set; }
    public string Zad1 { get; set; }
    public string Zad2 { get; set; }
    public string Zad3 { get; set; }
    public string Zad4 { get; set; }
    public string Zad5 { get; set; }
    public string Nlec1 { get; set; }
    public string Nlec2 { get; set; }
    public string Nlec3 { get; set; }
    public string Nlec4 { get; set; }
    public string Nlec5 { get; set; }
    public string Nlec6 { get; set; }
    public string Nlec7 { get; set; }
    public string Nlec8 { get; set; }
    public string Nlec9 { get; set; }
    public string Nlec10 { get; set; }
    public string Nlec11 { get; set; }
    public string Nlec12 { get; set; }
    public string Nlec13 { get; set; }
    public string Nlec14 { get; set; }
    public string Nlec15 { get; set; }
    public string Nlec16 { get; set; }
    public string Npract1 { get; set; }
    public string Npract2 { get; set; }
    public string Npract3 { get; set; }
    public string Npract4 { get; set; }
    public string Npract5 { get; set; }
    public string Npract6 { get; set; }
    public string Npract7 { get; set; }
    public string Npract8 { get; set; }
    public string Npract9 { get; set; }
    public string Npract10 { get; set; }
    public string Npract11 { get; set; }
    public string Npract12 { get; set; }
    public string Npract13 { get; set; }
    public string Npract14 { get; set; }
    public string Npract15 { get; set; }
    public string Npract16 { get; set; }
    public string Npract17 { get; set; }
    public string Npract18 { get; set; }
    public string Npract19 { get; set; }
    public string Npract20 { get; set; }
    public string Nlab1 { get; set; }
    public string Nlab2 { get; set; }
    public string Nlab3 { get; set; }
    public string Nlab4 { get; set; }
    public string Nlab5 { get; set; }
    public string Nlab6 { get; set; }
    public string Nlab7 { get; set; }
    public string Nlab8 { get; set; }
    public string Nlab9 { get; set; }
    public string Nlab10 { get; set; }
    public string Nlab11 { get; set; }
    public string Nlab12 { get; set; }
    public string Nlab13 { get; set; }
    public string Nlab14 { get; set; }
    public string Nlab15 { get; set; }
    public string Nlab16 { get; set; }
    public string Nlab17 { get; set; }
    public string Nlab18 { get; set; }
    public string Nlab19 { get; set; }
    public string Nlab20 { get; set; }
    public string Nsr1 { get; set; }
    public string Nsr2 { get; set; }
    public string Nsr3 { get; set; }
    public string Nsr4 { get; set; }
    public string Nsr5 { get; set; }
    public string OsnLitra1 { get; set; }
    public string OsnLitra2 { get; set; }
    public string OsnLitra3 { get; set; }
    public string OsnLitra4 { get; set; }
    public string OsnLitra5 { get; set; }
    public string DopLitra1 { get; set; }
    public string DopLitra2 { get; set; }
    public string DopLitra3 { get; set; }
    public string DopLitra4 { get; set; }
    public string DopLitra5 { get; set; }
    public string LecAnnotir1 { get; set; }
    public string LecAnnotir2 { get; set; }
    public string LecAnnotir3 { get; set; }
    public string LecAnnotir4 { get; set; }
    public string LecAnnotir5 { get; set; }
    public string LecAnnotir6 { get; set; }
    public string LecAnnotir7 { get; set; }
    public string LecAnnotir8 { get; set; }
    public string LecAnnotir9 { get; set; }
    public string LecAnnotir10 { get; set; }
    public string LecAnnotir11 { get; set; }
    public string LecAnnotir12 { get; set; }
    public string LecAnnotir13 { get; set; }
    public string LecAnnotir14 { get; set; }
    public string LecAnnotir15 { get; set; }
    public string LecAnnotir16 { get; set; }
    public string KursRab1 { get; set; }
    public string KursRab2 { get; set; }
    public string KursRab3 { get; set; }
    public string KursRab4 { get; set; }
    public string KursRab5 { get; set; }
    public string KursRab6 { get; set; }
    public string KursRab7 { get; set; }
    public string KursRab8 { get; set; }
    public string KursRab9 { get; set; }
    public string KursRab10 { get; set; }
    public string Fos1 { get; set; }
    public string Fos2 { get; set; }
    public string Fos3 { get; set; }
    public string Fos4 { get; set; }
    public string Fos5 { get; set; }
    public string Fos6 { get; set; }
    public string Fos7 { get; set; }
    public string Fos8 { get; set; }
    public string Fos9 { get; set; }
    public string Fos10 { get; set; }
    public string Fos11 { get; set; }
    public string Fos12 { get; set; }
    public string Fos13 { get; set; }
    public string Fos14 { get; set; }
    public string Fos15 { get; set; }
    public string Fos16 { get; set; }
    public string Fos17 { get; set; }
    public string Fos18 { get; set; }
    public string Fos19 { get; set; }
    public string Fos20 { get; set; }
    public string FosItog1 { get; set; }
    public string FosItog2 { get; set; }
    public string FosItog3 { get; set; }
    public string FosItog4 { get; set; }
    public string FosItog5 { get; set; }
    public string FosItog6 { get; set; }
    public string FosItog7 { get; set; }
    public string FosItog8 { get; set; }
    public string FosItog9 { get; set; }
    public string FosItog10 { get; set; }
    public string FosItog11 { get; set; }
    public string FosItog12 { get; set; }
    public string FosItog13 { get; set; }
    public string FosItog14 { get; set; }
    public string FosItog15 { get; set; }
    public string FosItog16 { get; set; }
    public string FosItog17 { get; set; }
    public string FosItog18 { get; set; }
    public string FosItog19 { get; set; }
    public string FosItog20 { get; set; }
    public string Index { get; set; }
}

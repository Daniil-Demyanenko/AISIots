using AISIots.Models.DbTables;
using ClosedXML.Excel;
using AISIots.Interfaces;

namespace AISIots.Services;

public class RpdParser : IExcelParser<Rpd>
{
    private readonly string _path;
    private readonly XLWorkbook? _wb;
    private readonly IXLWorksheet _ws;

    public RpdParser(ExcelPatternMatchingResult info, string path)
    {
        if (info.Type != ExcelFileType.Rpd) throw new Exception("this is not a RPD");
        if (info.WorksheetPosition == -1) throw new Exception("Important page not found");
        _wb = new XLWorkbook(path);
        _ws = _wb.Worksheet(info.WorksheetPosition);
        _path = path;
    }

    public Rpd Parse()
    {
        var fieldsInFile = ReadAsDictionary();

        return FillRpd(fieldsInFile);
    }

    private Dictionary<string, string> ReadAsDictionary()
    {
        var result = new Dictionary<string, string>();
        var title = Path.GetFileNameWithoutExtension(_path);
        result.Add("title", title);

        var lastRowUsed = _ws.LastRowUsed();
        if (lastRowUsed == null) return result;

        int rowUsedCount = lastRowUsed.RowNumber();
        for (var i = 1; i < rowUsedCount; i++)
        {
            var cell1 = _ws.Cell(i, 1);
            var cell2 = _ws.Cell(i, 2);

            var val1 = cell1.Value.ToString().Trim();
            var val2 = cell2.Value.ToString().Trim();
            if (!cell1.WorksheetRow().IsHidden && val1.Length > 2 && val2.Length != 0)
                result.Add(val1.ToLower(), val2);
        }

        return result;
    }

    private static Rpd FillRpd(Dictionary<string, string> fields)
    {
        var rpd = new Rpd();

        foreach (var key in fields.Keys)
        {
            var value = fields[key];

            if (TryAssignScalar(key, value, rpd)) continue;
            if (TryAddToList(key, value, rpd)) continue;
        }

        rpd.Title = fields["title"];

        return rpd;
    }

    private static bool TryAssignScalar(string key, string value, Rpd rpd)
    {
        if (key.Contains("fosformakontrol"))      { rpd.FosFormaKontrol = value; return true; }
        if (key.Contains("foskomp"))              { rpd.FosKomp = value; return true; }
        if (key.Contains("formakontrol11"))       { rpd.FormaKontrol11 = value; return true; }
        if (key.Contains("formaobuch"))           { rpd.FormaObuch = value; return true; }
        if (key.Contains("prepodregfullshort"))   { rpd.PrepodRegFullShort = value; return true; }
        if (key.Contains("prepodregfull") && key.Length <= 14) { rpd.PrepodRegFull = value; return true; }
        if (key.Contains("razrabshort"))          { rpd.RazrabShort = value; return true; }
        if (key.Contains("razrab") && key.Length <= 7) { rpd.Razrab = value; return true; }
        if (key.Contains("dopprogrobesp"))        { rpd.DopProgObesp = value; return true; }
        if (key.Contains("tekkontrol"))           { rpd.TekKontrol = value; return true; }
        if (key.Contains("umkpreds"))             { rpd.UmkPreds = value; return true; }
        if (key.Contains("protumk"))              { rpd.ProtUmk = value; return true; }
        if (key.Contains("protkaf"))              { rpd.ProtKaf = value; return true; }
        if (key.Contains("fakshort"))             { rpd.FakShort = value; return true; }
        if (key.Contains("fak"))                  { rpd.Fak = value; return true; }
        if (key.Contains("koddisc"))              { rpd.KodDisc = value; return true; }
        if (key.Contains("kodkaf"))               { rpd.KodKaf = value; return true; }
        if (key.Contains("kodspec"))              { rpd.KodSpec = value; return true; }
        if (key.Contains("kvalif"))               { rpd.Kvalif = value; return true; }
        if (key.Contains("standart"))             { rpd.Standart = value; return true; }
        if (key.Contains("zavkaf"))               { rpd.ZavKaf = value; return true; }
        if (key.Contains("dirfio"))               { rpd.DirFio = value; return true; }
        if (key.Contains("semshort"))             { rpd.SemShort = value; return true; }
        if (key.Contains("samrabzo"))             { rpd.SamRabZo = value; return true; }
        if (key.Contains("samrab"))               { rpd.SamRab = value; return true; }
        if (key.Contains("sldla"))                { rpd.Sldla = value; return true; }
        if (key.Contains("profil"))               { rpd.Profil = value; return true; }
        if (key.Contains("spec"))                 { rpd.Spec = value; return true; }
        if (key.Contains("tceli"))                { rpd.Tceli = value; return true; }
        if (key.Contains("znat"))                 { rpd.Znat = value; return true; }
        if (key.Contains("umet"))                 { rpd.Umet = value; return true; }
        if (key.Contains("vladet"))               { rpd.Vladet = value; return true; }
        if (key.Contains("osnna"))                { rpd.Osnna = value; return true; }
        if (key.Contains("chast"))                { rpd.Chast = value; return true; }
        if (key.Contains("itogo"))                { rpd.Itogo = value; return true; }
        if (key.Contains("ksrzo"))                { rpd.KsrZo = value; return true; }
        if (key.Contains("ksr"))                  { rpd.Ksr = value; return true; }
        if (key.Contains("sem") && !key.Contains("semshort") && !key.Contains("semsh"))  { rpd.Sem = value; return true; }
        if (key.Contains("ze"))                   { rpd.Ze = value; return true; }
        if (key.Contains("kaf"))                  { rpd.Kaf = value; return true; }
        if (key.Contains("komp1n123et"))           { rpd.Komp1N123Et = value; return true; }
        if (key.Contains("komp2n123et"))           { rpd.Komp2N123Et = value; return true; }
        if (key.Contains("komp3n123et"))           { rpd.Komp3N123Et = value; return true; }
        if (key.Contains("komp1n123"))             { rpd.Komp1N123 = value; return true; }
        if (key.Contains("komp2n123"))             { rpd.Komp2N123 = value; return true; }
        if (key.Contains("komp3n123"))             { rpd.Komp3N123 = value; return true; }
        if (key.Contains("komp") && key.Length <= 7) { rpd.Komp.Add(value); return true; }
        if (key.Contains("komp1n"))                { rpd.Komp1n = value; return true; }
        if (key.Contains("komp2n"))                { rpd.Komp2n = value; return true; }
        if (key.Contains("komp3n"))                { rpd.Komp3n = value; return true; }
        return false;
    }

    private static bool TryAddToList(string key, string value, Rpd rpd)
    {
        if (key.Contains("fosito"))             { rpd.FosItog.Add(value); return true; }
        if (key.Contains("fos") && key.Length <= 6) { rpd.Fos.Add(value); return true; }
        if (key.Contains("lecannot"))           { rpd.LecAnnotir.Add(value); return true; }
        if (key.Contains("kursra"))             { rpd.KursRab.Add(value); return true; }
        if (key.Contains("doplitra"))           { rpd.DopLitra.Add(value); return true; }
        if (key.Contains("osnlitra"))           { rpd.OsnLitra.Add(value); return true; }
        if (key.Contains("nnsr"))               { rpd.Nnsr.Add(ParseIntOrZero(value)); return true; }
        if (key.Contains("wnsr"))               { rpd.Wnsr.Add(ParseIntOrZero(value)); return true; }
        if (key.Contains("nsr"))                { rpd.Nsr.Add(value); return true; }
        if (key.Contains("npract"))             { rpd.Npract.Add(value); return true; }
        if (key.Contains("nlab"))               { rpd.Nlab.Add(value); return true; }
        if (key.Contains("nlec"))               { rpd.Nlec.Add(value); return true; }
        if (key.Contains("zad"))                { rpd.Zad.Add(value); return true; }
        if (key.Contains("xlzo"))               { rpd.Xlzo.Add(ParseIntOrZero(value)); return true; }
        if (key.Contains("zlzo"))               { rpd.Zlzo.Add(ParseIntOrZero(value)); return true; }
        if (key.Contains("ylzo"))               { rpd.Ylzo.Add(ParseIntOrZero(value)); return true; }
        if (key.Contains("xl"))                 { rpd.Xl.Add(ParseIntOrZero(value)); return true; }
        if (key.Contains("zl"))                 { rpd.Zl.Add(ParseIntOrZero(value)); return true; }
        if (key.Contains("yl"))                 { rpd.Yl.Add(ParseIntOrZero(value)); return true; }
        return false;
    }

    private static int ParseIntOrZero(string value) =>
        int.TryParse(value, out var result) ? result : 0;

    public void Dispose()
    {
        _wb?.Dispose();
    }
}
using AISIots.Models.DbTables;
using ClosedXML.Excel;

namespace AISIots.Services;

public class RpdParser : IDisposable
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

        int rowUsedCount = _ws.LastRowUsed().RowNumber();
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

    private Rpd FillRpd(Dictionary<string, string> fields)
    {
        var rpd = new Rpd();
        foreach (var key in fields.Keys)
        {
            if (key.Contains("fos") && key.Length <= 6) rpd.Fos.Add(fields[key]);
            if (key.Contains("fosito")) rpd.FosItog.Add(fields[key]);
            if (key.Contains("lecannot")) rpd.LecAnnotir.Add(fields[key]);
            if (key.Contains("kursra")) rpd.KursRab.Add(fields[key]);
            if (key.Contains("doplitra")) rpd.DopLitra.Add(fields[key]);
            if (key.Contains("osnlitra")) rpd.OsnLitra.Add(fields[key]);
            if (key.Contains("nsr")) rpd.Nsr.Add(fields[key]);
            if (key.Contains("npract")) rpd.Npract.Add(fields[key]);
            if (key.Contains("nlab")) rpd.Nlab.Add(fields[key]);
            if (key.Contains("nlec")) rpd.Nlec.Add(fields[key]);
            if (key.Contains("zad")) rpd.Zad.Add(fields[key]);

            if (key.Contains("prepodregfull") && key.Length <= 14) rpd.PrepodRegFull = fields[key];
            if (key.Contains("prepodregfullshort")) rpd.PrepodRegFullShort = fields[key];
            if (key.Contains("razrab") && key.Length <= 7) rpd.Razrab = fields[key];
            if (key.Contains("razrabshort")) rpd.RazrabShort = fields[key];
            if (key.Contains("tceli")) rpd.Tceli = fields[key];
            if (key.Contains("znat")) rpd.Znat = fields[key];
            if (key.Contains("umet")) rpd.Umet = fields[key];
            if (key.Contains("vladet")) rpd.Vladet = fields[key];
            if (key.Contains("osnna")) rpd.Osnna = fields[key];
            if (key.Contains("sldla")) rpd.Sldla = fields[key];
            if (key.Contains("dopprogrobesp")) rpd.DopProgObesp = fields[key];
        }

        rpd.Title = fields["title"];

        return rpd;
    }

    public void Dispose()
    {
        _wb?.Dispose();
    }
}
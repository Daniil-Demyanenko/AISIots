namespace AISIots.ViewModels;

public class UploadExcelFilesModel
{
    public bool LoadSuccessful { get; set; }
    public IEnumerable<string> SuccessFiles { get; set; } = new List<string>();
    public IEnumerable<string> ProblemFiles { get; set; } = new List<string>();
}

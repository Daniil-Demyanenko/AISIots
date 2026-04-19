namespace AISIots.ViewModels;

public class SearchModel
{
    public IEnumerable<SearchItem> Items { get; set; } = new List<SearchItem>();
    public bool IsRpdSearch { get; set; }
}

public record SearchItem(int Id, string Title);

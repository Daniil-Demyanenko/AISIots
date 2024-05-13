using AISIots.Services;

namespace AISIots.Models;

public class SearchModel(IEnumerable<SearchItem> items, bool isRpdSearch)
{
    public IEnumerable<SearchItem> Items = items;
    public bool IsRpdSearch = isRpdSearch;
}

public record SearchItem(int Id, string Title);
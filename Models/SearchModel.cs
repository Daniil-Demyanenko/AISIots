using AISIots.DAL;
using AISIots.Services;

namespace AISIots.Models;

public class SearchModel(IEnumerable<SearchItem> items, bool isRpdSearch)
{
    public IEnumerable<SearchItem> Items = items;
    public bool IsRpdSearch = isRpdSearch;

    public static SearchModel Create(SqliteContext db, string? searchString = null, bool isRpdSearch = true)
    {
        var searcher = new FuzzyService(db);

        if (string.IsNullOrEmpty(searchString?.Trim()))
            return searcher.GetNewestRpds();

        return searcher.GetFuzzySorted(searchString, isRpdSearch);
    }
}

public record SearchItem(int Id, string Title);
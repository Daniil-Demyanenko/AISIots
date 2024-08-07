using AISIots.DAL;
using FuzzySharp;

namespace AISIots.Models;

public class SearchModel(IEnumerable<SearchItem> items, bool isRpdSearch)
{
    public IEnumerable<SearchItem> Items = items;
    public bool IsRpdSearch = isRpdSearch;

    public static SearchModel Create(SqliteContext db, string? searchString = null, bool isRpdSearch = true)
        => string.IsNullOrEmpty(searchString?.Trim()) ? GetNewestRpds(db) : GetFuzzySorted(searchString, isRpdSearch, db);


    private static SearchModel GetNewestRpds(SqliteContext db)
    {
        var result = db.Rpds
            .OrderByDescending(x => x.UpdateDateTime).Take(50)
            .Select(x => new SearchItem(x.Id, x.Title));
        return new SearchModel(result, isRpdSearch: true);
    }

    private static SearchModel GetFuzzySorted(string searchString, bool isRpdSearch, SqliteContext db)
    {
        var select = isRpdSearch
            ? db.Rpds.Select(x => new SearchItem(x.Id, x.Title))
            : db.Plans.Select(x => new SearchItem(x.Id, $"{GetFirstPart(x.Code)} - {x.Profile} ({x.GroupYear})"));

        var items = select.AsEnumerable().OrderByDescending(x => Fuzz.TokenSortRatio(searchString.ToLower(), x.Title.ToLower())).Take(50);

        return new SearchModel(items, isRpdSearch);
    }

    private static string GetFirstPart(string? str)
        => str?.Split(' ')[0] ?? "";
}

public record SearchItem(int Id, string Title);
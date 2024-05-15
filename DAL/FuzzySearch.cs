using AISIots.DAL;
using AISIots.Models;
using FuzzySharp;

namespace AISIots.Services;

public class FuzzyService(SqliteContext db)
{
    public SearchModel GetNewestRpds()
    {
        var result = db.Rpds!
            .OrderByDescending(x => x.UpdateDateTime).Take(50)
            .Select(x => new SearchItem(x.Id, x.Title));
        return new SearchModel(result, isRpdSearch: true);
    }

    public SearchModel GetFuzzySorted(string searchString, bool isRpdSearch)
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
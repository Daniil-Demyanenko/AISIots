using AISIots.Models;
using AISIots.ViewModels;
using AISIots.Models.DbTables;

namespace AISIots.Interfaces;

public interface IPlanService
{
    SearchModel Search(string? searchString, bool isRpdSearch);
    Plan? GetPlanById(int id);
    Task<string?> UpdateRpdAsync(Rpd rpd);
    Task<string?> CreateRpdAsync(Rpd rpd);
    Task DeleteRpdAsync(int? id);
    Task<Rpd> FindOrCreateRpdByIdAsync(int? id);
}

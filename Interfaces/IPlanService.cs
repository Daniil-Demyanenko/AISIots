using AISIots.Models;
using AISIots.ViewModels;
using AISIots.Models.DbTables;

namespace AISIots.Interfaces;

public interface IPlanService
{
    Task<SearchModel> Search(string? searchString, bool isRpdSearch);
    Task<Plan?> GetPlanByIdAsync(int id);
    Task<Plan> GetPlanByIdOrThrowAsync(int id);
    Task<string?> UpdateRpdAsync(Rpd rpd);
    Task<string?> CreateRpdAsync(Rpd rpd);
    Task DeleteRpdAsync(int? id);
    Task DeletePlanAsync(int id);
    Task<Rpd> FindOrCreateRpdByIdAsync(int? id);
}
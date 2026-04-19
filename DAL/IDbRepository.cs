using AISIots.Models.DbTables;

namespace AISIots.DAL;

public interface IDbRepository
{
    bool IsContainLogicalSamePlan(Plan plan);

    bool IsContainRpdWithTitle(string title);

    bool IsContainRpdWithSameTitleDifferentId(string title, int id);

    /// <summary>
    /// Возвращает список РПД, которые упомянуты в планах, отсутствуют в БД
    /// </summary>
    /// <returns></returns>
    Task<IOrderedEnumerable<string>> GetMissingRpds();
}
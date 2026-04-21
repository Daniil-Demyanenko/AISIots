using AISIots.Models.DbTables;
using System.IO;
using System.Threading.Tasks;

namespace AISIots.Interfaces
{
    public interface ITemplateGeneratorService
    {
        Task<(Stream stream, string fileName)> GenerateDocumentAsync(Rpd rpd, string templateName);
    }
}
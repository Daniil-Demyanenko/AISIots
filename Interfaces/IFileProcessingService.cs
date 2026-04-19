using Microsoft.AspNetCore.Http;
using AISIots.Models;
using AISIots.ViewModels;

namespace AISIots.Interfaces;

public interface IFileProcessingService
{
    Task<UploadExcelFilesModel> ProcessUploadedFilesAsync(List<IFormFile>? files);
}

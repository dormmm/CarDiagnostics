using System.Threading.Tasks;

namespace CarDiagnostics.Domain.Models.Interfaces
{
    public interface IAzureStorageService
    {
        Task<string?> DownloadFileAsync(string fileName);
        Task UploadFileAsync(string fileName, string content);
    }
}

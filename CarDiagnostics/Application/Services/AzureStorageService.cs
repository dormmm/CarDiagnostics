using Azure.Storage.Blobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using CarDiagnostics.Domain.Models.Interfaces;

namespace CarDiagnostics.Services
{
    public class AzureStorageService : IAzureStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly IMemoryCache _cache;

        public AzureStorageService(IConfiguration configuration, IMemoryCache cache)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"];
            _containerName = configuration["AzureStorage:ContainerName"];
            _cache = cache;
        }

        public async Task UploadFileAsync(string fileName, string content)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            await blobClient.UploadAsync(stream, overwrite: true);

            // נעדכן גם את הקאש כדי לשקף את הגרסה החדשה
            _cache.Set(fileName, content, TimeSpan.FromHours(1));
        }

        public async Task<string> DownloadFileAsync(string fileName)
        {
            // נבדוק אם יש את הקובץ בזיכרון
            if (_cache.TryGetValue(fileName, out string cachedContent))
            {
                return cachedContent;
            }

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                var downloadResult = await blobClient.DownloadContentAsync();
                var content = downloadResult.Value.Content.ToString();

                // נשמור את הקובץ בזיכרון לשעה
                _cache.Set(fileName, content, TimeSpan.FromHours(1));

                return content;
            }

            return null;
        }
    }
}

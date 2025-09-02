using CarDiagnostics.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CarDiagnostics.Domain.Interfaces;
using CarDiagnostics.Domain.Models.Interfaces; // IAzureStorageService
using System;

namespace CarDiagnostics.Repository
{
    public class CarsCallsRepository : ICarsCallsRepository
    {
        private readonly IAzureStorageService _storageService;
        private readonly ILogger<CarsCallsRepository> _logger;
        private readonly string _fileName = "carsCalls.json";

        private List<Car>? _cachedCalls;
        private DateTime _cacheTimestamp;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public CarsCallsRepository(IAzureStorageService storageService, ILogger<CarsCallsRepository> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<List<Car>> ReadCallsAsync()
        {
            try
            {
                if (_cachedCalls != null && DateTime.UtcNow - _cacheTimestamp < _cacheDuration)
                    return _cachedCalls;

                var json = await _storageService.DownloadFileAsync(_fileName);

                _cachedCalls = string.IsNullOrWhiteSpace(json)
                    ? new List<Car>()
                    : JsonConvert.DeserializeObject<List<Car>>(json!) ?? new List<Car>();

                _cacheTimestamp = DateTime.UtcNow;
                return _cachedCalls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading carsCalls from Azure Blob");
                return new List<Car>();
            }
        }

        public async Task SaveCallsAsync(List<Car> calls)
        {
            try
            {
                var json = JsonConvert.SerializeObject(calls, Formatting.Indented);

                // ✅ שמירה ל-Azure בלבד (בלי קובץ לוקאלי)
                await _storageService.UploadFileAsync(_fileName, json);

                _cachedCalls = calls;
                _cacheTimestamp = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving carsCalls to Azure Blob");
            }
        }
    }
}

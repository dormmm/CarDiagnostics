using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CarDiagnostics.Models;
using CarDiagnostics.Domain.Interfaces;
using System.Threading;
using CarDiagnostics.Services;

namespace CarDiagnostics.Repository
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AzureStorageService _storageService;
        private readonly ILogger<VehicleRepository> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly string _fileName = "vehiclesList.json";


        private VehicleList _cachedVehicles = null!;
        private DateTime _lastLoadTime = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public VehicleRepository(AzureStorageService storageService, ILogger<VehicleRepository> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        private string NormalizeModel(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input ?? "";

            input = input.Trim();
            var firstWord = input.Split(' ')[0].ToLower();

            return char.ToUpper(firstWord[0]) + firstWord.Substring(1);
        }

        public async Task<VehicleList> GetAllVehiclesAsync()
        {
            if (_cachedVehicles != null && DateTime.UtcNow - _lastLoadTime < _cacheDuration)
                return _cachedVehicles;

            await _semaphore.WaitAsync();
            try
            {
                var json = await _storageService.DownloadFileAsync(_fileName);
                _cachedVehicles = string.IsNullOrEmpty(json)
                    ? new VehicleList()
                    : JsonConvert.DeserializeObject<VehicleList>(json) ?? new VehicleList();

                _lastLoadTime = DateTime.UtcNow;
                return _cachedVehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading vehicles list from Azure Blob");
                return new VehicleList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsCompanyExistsAsync(string company)
        {
            var data = await GetAllVehiclesAsync();
            return data.ContainsKey(company);
        }

        public async Task<bool> IsModelExistsAsync(string company, string model)
        {
            var data = await GetAllVehiclesAsync();
            return data.ContainsKey(company) && data[company].Any(m => m.model == model);
        }

        public async Task<bool> IsModelExistsNormalizedAsync(string company, string model)
        {
            var data = await GetAllVehiclesAsync();

            if (!data.ContainsKey(company))
                return false;

            var normalizedInput = NormalizeModel(model);
            return data[company].Any(m => NormalizeModel(m.model) == normalizedInput);
        }

        public async Task<List<string>> GetModelsByCompanyAsync(string company)
        {
            var data = await GetAllVehiclesAsync();
            return data.ContainsKey(company)
                ? data[company].Select(m => m.model).ToList()
                : new List<string>();
        }

        public async Task<List<string>> GetAllCompaniesAsync()
        {
            var data = await GetAllVehiclesAsync();
            return data.Keys.ToList();
        }
    }
}

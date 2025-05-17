using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CarDiagnostics.Models;
using CarDiagnostics.Domain.Interfaces;
using System.Threading;

namespace CarDiagnostics.Repository
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly string _filePath;
        private readonly ILogger<VehicleRepository> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private VehicleList _cachedVehicles = null!;
        private DateTime _lastLoadTime = DateTime.MinValue;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public VehicleRepository(IConfiguration configuration, ILogger<VehicleRepository> logger)
        {
            _filePath = configuration["FilePaths:Vehicles"] ?? throw new Exception("Missing config for FilePaths:Vehicles");
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

public async Task<bool> IsModelExistsNormalizedAsync(string company, string model)
{
    var data = await GetAllVehiclesAsync();

    if (!data.ContainsKey(company))
        return false;

    var normalizedInput = NormalizeModel(model);

    return data[company].Any(m => NormalizeModel(m.model) == normalizedInput);
}



        public async Task<VehicleList> GetAllVehiclesAsync()
        {
            if (_cachedVehicles != null && DateTime.UtcNow - _lastLoadTime < _cacheDuration)
            {
                return _cachedVehicles;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_cachedVehicles != null && DateTime.UtcNow - _lastLoadTime < _cacheDuration)
                {
                    return _cachedVehicles;
                }

                if (!File.Exists(_filePath))
                {
                    _cachedVehicles = new VehicleList();
                    _lastLoadTime = DateTime.UtcNow;
                    return _cachedVehicles;
                }

                var json = await File.ReadAllTextAsync(_filePath);
                _cachedVehicles = JsonConvert.DeserializeObject<VehicleList>(json) ?? new VehicleList();
                _lastLoadTime = DateTime.UtcNow;

                return _cachedVehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading vehicles list from {FilePath}", _filePath);
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

        public async Task<List<string>> GetModelsByCompanyAsync(string company)
        {
            var data = await GetAllVehiclesAsync();

            if (data.ContainsKey(company))
            {
                return data[company].Select(m => m.model).ToList();
            }

            return new List<string>();
        }

        public async Task<List<string>> GetAllCompaniesAsync()
        {
            var data = await GetAllVehiclesAsync();
            return data.Keys.ToList();
        }

        
    }
}

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CarDiagnostics.Models;

namespace CarDiagnostics.Repository
{
    public class VehicleRepository
    {
        private readonly string _filePath;
        private readonly ILogger<VehicleRepository> _logger;

        public VehicleRepository(IConfiguration configuration, ILogger<VehicleRepository> logger)
        {
            _filePath = configuration["FilePaths:Vehicles"] ?? throw new Exception("Missing config for FilePaths:Vehicles");
            _logger = logger;
        }

        public async Task<VehicleList> GetAllVehiclesAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new VehicleList();

                var json = await File.ReadAllTextAsync(_filePath);
                return JsonConvert.DeserializeObject<VehicleList>(json) ?? new VehicleList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading vehicles list from {FilePath}", _filePath);
                return new VehicleList();
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

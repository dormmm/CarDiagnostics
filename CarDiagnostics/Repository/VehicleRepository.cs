using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CarDiagnostics.Models;

namespace CarDiagnostics.Repository
{
    public class VehicleRepository
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private readonly ILogger<VehicleRepository> _logger;

        public VehicleRepository(IConfiguration configuration, ILogger<VehicleRepository> logger)
        {
            _filePath = configuration["FilePaths:Vehicles"] ?? throw new Exception("Missing config for FilePaths:Vehicles");
            _logger = logger;
        }

        public VehicleList GetAllVehicles()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(_filePath))
                        return new VehicleList();

                    var json = File.ReadAllText(_filePath);
                    return JsonConvert.DeserializeObject<VehicleList>(json)
                        ?? new VehicleList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading vehicles list from {FilePath}", _filePath);
                    return new VehicleList();
                }
            }
        }

        public bool IsCompanyExists(string company)
        {
            var data = GetAllVehicles();
            return data.ContainsKey(company);
        }

        public bool IsModelExists(string company, string model)
        {
            var data = GetAllVehicles();
            return data.ContainsKey(company) && data[company].Any(m => m.model == model);
        }

        public List<string> GetModelsByCompany(string company)
        {
            var data = GetAllVehicles();

            if (data.ContainsKey(company))
            {
                return data[company].Select(m => m.model).ToList();
            }

            return new List<string>();
        }

        public List<string> GetAllCompanies()
        {
            var data = GetAllVehicles();
            return data.Keys.ToList();
        }
    }
}

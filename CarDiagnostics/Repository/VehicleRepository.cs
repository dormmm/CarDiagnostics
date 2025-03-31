using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;

namespace CarDiagnostics.Repository
{
    public class VehicleRepository
    {
        private readonly string _vehiclesListFilePath = "vehiclesList.json";
        private readonly object _lock = new object();
        private readonly ILogger<VehicleRepository> _logger;

        public VehicleRepository(ILogger<VehicleRepository> logger)
        {
            _logger = logger;
        }

        public Dictionary<string, List<dynamic>> GetAllVehicles()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(_vehiclesListFilePath))
                        return new Dictionary<string, List<dynamic>>();

                    var json = File.ReadAllText(_vehiclesListFilePath);
                    return JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(json)
                        ?? new Dictionary<string, List<dynamic>>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading vehicles list from {FilePath}", _vehiclesListFilePath);
                    return new Dictionary<string, List<dynamic>>();
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
            return data.ContainsKey(company) && data[company].Any(m => (string)m["model"] == model);
        }

        public List<string> GetModelsByCompany(string company)
        {
            var data = GetAllVehicles();

            if (data.ContainsKey(company))
            {
                return data[company].Select(m => (string)m["model"]).ToList();
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

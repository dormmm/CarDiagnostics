using CarDiagnostics.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CarDiagnostics.Repository
{
    public class CarsCallsRepository
    {
        private readonly string _filePath;
        private readonly ILogger<CarsCallsRepository> _logger;

        public CarsCallsRepository(IConfiguration configuration, ILogger<CarsCallsRepository> logger)
        {
            _filePath = configuration["FilePaths:CarsCalls"] ?? throw new Exception("Missing config for FilePaths:CarsCalls");
            _logger = logger;
        }

        public async Task<List<Car>> ReadCallsAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<Car>();

                var json = await File.ReadAllTextAsync(_filePath);
                return JsonConvert.DeserializeObject<List<Car>>(json) ?? new List<Car>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading carsCalls from {FilePath}", _filePath);
                return new List<Car>();
            }
        }

        public async Task SaveCallsAsync(List<Car> calls)
        {
            try
            {
                var json = JsonConvert.SerializeObject(calls, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving carsCalls to {FilePath}", _filePath);
            }
        }
    }
}

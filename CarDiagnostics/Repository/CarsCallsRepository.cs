using CarDiagnostics.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CarDiagnostics.Repository
{
    public class CarsCallsRepository
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private readonly ILogger<CarsCallsRepository> _logger;

        public CarsCallsRepository(IConfiguration configuration, ILogger<CarsCallsRepository> logger)
        {
            _filePath = configuration["FilePaths:CarsCalls"] ?? throw new Exception("Missing config for FilePaths:CarsCalls");
            _logger = logger;
        }

        public List<Car> ReadCalls()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(_filePath))
                        return new List<Car>();

                    var json = File.ReadAllText(_filePath);
                    return JsonConvert.DeserializeObject<List<Car>>(json) ?? new List<Car>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading carsCalls from {FilePath}", _filePath);
                    return new List<Car>();
                }
            }
        }

        public void SaveCalls(List<Car> calls)
        {
            lock (_lock)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(calls, Formatting.Indented);
                    File.WriteAllText(_filePath, json);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving carsCalls to {FilePath}", _filePath);
                }
            }
        }
    }
}

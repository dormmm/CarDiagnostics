using CarDiagnostics.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CarDiagnostics.Domain.Interfaces;

namespace CarDiagnostics.Repository
{
    public class CarsCallsRepository : ICarsCallsRepository
    {
        private readonly string _filePath;
        private readonly ILogger<CarsCallsRepository> _logger;

        // ✅ Cache
        private List<Car> _cachedCalls;
        private DateTime _cacheTimestamp;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public CarsCallsRepository(IConfiguration configuration, ILogger<CarsCallsRepository> logger)
        {
            _filePath = configuration["FilePaths:CarsCalls"] ?? throw new Exception("Missing config for FilePaths:CarsCalls");
            _logger = logger;
        }

        public async Task<List<Car>> ReadCallsAsync()
        {
            try
            {
                if (_cachedCalls != null && DateTime.Now - _cacheTimestamp < _cacheDuration)
                {
                    return _cachedCalls;
                }

                if (!File.Exists(_filePath))
                {
                    _cachedCalls = new List<Car>();
                    _cacheTimestamp = DateTime.Now;
                    return _cachedCalls;
                }

                var json = await File.ReadAllTextAsync(_filePath);
                _cachedCalls = JsonConvert.DeserializeObject<List<Car>>(json) ?? new List<Car>();
                _cacheTimestamp = DateTime.Now;

                return _cachedCalls;
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

                // ✅ עדכון cache אחרי שמירה
                _cachedCalls = calls;
                _cacheTimestamp = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving carsCalls to {FilePath}", _filePath);
            }
        }
    }
}

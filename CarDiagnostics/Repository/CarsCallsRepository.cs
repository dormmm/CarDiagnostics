using CarDiagnostics.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CarDiagnostics.Repository
{
    public class CarsCallsRepository
    {
        private readonly string _filePath = "carsCalls.json";
        private readonly object _lock = new object();

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
                    Console.WriteLine("Error reading carsCalls: " + ex.Message);
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
                    Console.WriteLine("Error saving carsCalls: " + ex.Message);
                }
            }
        }
    }
}

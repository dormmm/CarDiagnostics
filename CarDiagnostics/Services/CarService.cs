using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using CarDiagnostics.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CarDiagnostics.Services
{
    public class CarService
    {
        private readonly string _usersFilePath = "users.json";
        private readonly string _carsCallsFilePath = "carsCalls.json";
        private readonly string _vehiclesListFilePath = "vehiclesList.json";
        private readonly AIService _aiService;

        public CarService(AIService aiService)
        {
            _aiService = aiService;
        }

        public List<User> ReadUsersFromFile()
        {
            if (!File.Exists(_usersFilePath)) return new List<User>();

            var json = File.ReadAllText(_usersFilePath);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }

        public Dictionary<string, List<dynamic>> ReadVehiclesListFromFile()
        {
            if (!File.Exists(_vehiclesListFilePath)) return new Dictionary<string, List<dynamic>>();

            var json = File.ReadAllText(_vehiclesListFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(json) ?? new Dictionary<string, List<dynamic>>();
        }

        public List<Car> ReadCarsCallsFromFile()
        {
            if (!File.Exists(_carsCallsFilePath)) return new List<Car>();

            var json = File.ReadAllText(_carsCallsFilePath);
            return JsonConvert.DeserializeObject<List<Car>>(json) ?? new List<Car>();
        }

        public void SaveCarsCallsToFile(List<Car> cars)
        {
            var json = JsonConvert.SerializeObject(cars, Formatting.Indented);
            File.WriteAllText(_carsCallsFilePath, json);
        }

        public bool IsUserValid(string username, string email)
        {
            var users = ReadUsersFromFile();
            return users.Any(u => u.Username == username && u.Email == email);
        }

        public async Task<IActionResult> SubmitProblemAsync(string username, string email, string company, string model, int year, string problemDescription)
        {
            if (!IsUserValid(username, email))
            {
                return new BadRequestObjectResult("User not found in the system.");
            }

            var vehiclesData = ReadVehiclesListFromFile();
            if (!vehiclesData.ContainsKey(company) || !vehiclesData[company].Any(m => (string)m["model"] == model))
            {
                return new BadRequestObjectResult("Company or model not found in the system.");
            }

            var diagnosis = await _aiService.GetDiagnosisAsync(company, model, year, problemDescription);

            var carCall = new Car
            {
                Username = username,
                Email = email,
                Company = company,
                Model = model,
                Year = year,
                ProblemDescription = problemDescription,
                AIResponse = diagnosis
            };

            var carCalls = ReadCarsCallsFromFile();
            carCalls.Add(carCall);
            SaveCarsCallsToFile(carCalls);

            return new OkObjectResult(new { Message = "Problem submitted successfully!", AI_Diagnosis = diagnosis });
        }
    }
}

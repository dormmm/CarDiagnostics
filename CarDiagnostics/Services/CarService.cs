using CarDiagnostics.Models;
using CarDiagnostics.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarDiagnostics.Services
{
    public class CarService
    {
        private readonly AIService _aiService;
        private readonly UserRepository _userRepository;
        private readonly VehicleRepository _vehicleRepository;
        private readonly CarsCallsRepository _carsCallsRepository;

        public CarService(AIService aiService, UserRepository userRepository, VehicleRepository vehicleRepository, CarsCallsRepository carsCallsRepository)
        {
            _aiService = aiService;
            _userRepository = userRepository;
            _vehicleRepository = vehicleRepository;
            _carsCallsRepository = carsCallsRepository;
        }

        public async Task<List<string>> GetAllCarCompaniesAsync()
        {
            var data = await _vehicleRepository.GetAllVehiclesAsync();
            return data.Keys.ToList();
        }

        public async Task<List<string>> GetCarModelsByCompanyAsync(string company)
        {
            return await _vehicleRepository.GetModelsByCompanyAsync(company);
        }

        public async Task<bool> IsCompanyExistsAsync(string company)
        {
            return await _vehicleRepository.IsCompanyExistsAsync(company);
        }

        public async Task<bool> IsModelExistsAsync(string company, string model)
        {
            var companyModels = await _vehicleRepository.GetModelsByCompanyAsync(company);
            return companyModels.Contains(model);
        }

        public async Task<IActionResult> SubmitProblemAsync(string username, string email, string company, string model, int year, string problemDescription)
        {
            if (!await _userRepository.IsValidUserAsync(username, email))
            {
                return new BadRequestObjectResult("User not found in the system.");
            }

            if (!await _vehicleRepository.IsModelExistsAsync(company, model))
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

            var existingCalls = await _carsCallsRepository.ReadCallsAsync();
            existingCalls.Add(carCall);
            await _carsCallsRepository.SaveCallsAsync(existingCalls);

            return new OkObjectResult(new { Message = "Problem submitted successfully!", AI_Diagnosis = diagnosis });
        }
    }
}

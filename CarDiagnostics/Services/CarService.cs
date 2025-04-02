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

        public List<string> GetAllCarCompanies()
        {
            return _vehicleRepository.GetAllVehicles().Keys.ToList();
        }

        public List<string> GetCarModelsByCompany(string company)
        {
            return _vehicleRepository.GetModelsByCompany(company);
        }

        public bool IsCompanyExists(string company)
        {
            return _vehicleRepository.IsCompanyExists(company);
        }
        public bool IsModelExists(string company, string model)
{
    var companyModels = _vehicleRepository.GetModelsByCompany(company);
    return companyModels.Contains(model);
}


        public async Task<IActionResult> SubmitProblemAsync(string username, string email, string company, string model, int year, string problemDescription)
        {
            if (!_userRepository.IsValidUser(username, email))
            {
                return new BadRequestObjectResult("User not found in the system.");
            }

            if (!_vehicleRepository.IsModelExists(company, model))
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

            var existingCalls = _carsCallsRepository.ReadCalls();
            existingCalls.Add(carCall);
            _carsCallsRepository.SaveCalls(existingCalls);

            return new OkObjectResult(new { Message = "Problem submitted successfully!", AI_Diagnosis = diagnosis });
        }
    }
}

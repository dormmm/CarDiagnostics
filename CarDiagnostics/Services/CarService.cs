using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using CarDiagnostics.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace CarDiagnostics.Services
{
    public class CarService
    {
        private readonly string _carsFilePath = "vehicles.json"; // קובץ הרכבים
        private readonly string _carsCallsFilePath = "carsCalls.json"; // קובץ הקריאות
        private readonly string _vehiclesListFilePath = "vehiclesList.json"; // קובץ החברות והדגמים

          // קריאה מקובץ vehiclesList.json (חברות ודגמים בלבד)
        public Dictionary<string, List<dynamic>> ReadVehiclesListFromFile()
        {
            if (!File.Exists(_vehiclesListFilePath))
            {
                return new Dictionary<string, List<dynamic>>(); // אם אין קובץ, מחזירים רשימה ריקה
            }

            var json = File.ReadAllText(_vehiclesListFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(json) ?? new Dictionary<string, List<dynamic>>();
        }

         // בדיקה אם חברה קיימת
        public bool IsCompanyExists(string company)
        {
            var vehiclesData = ReadVehiclesListFromFile();
            return vehiclesData.ContainsKey(company);
        }

        // בדיקה אם דגם רכב קיים תחת חברה
        public bool IsCarModelExists(string company, string model)
        {
            var vehiclesData = ReadVehiclesListFromFile();
            return vehiclesData.ContainsKey(company) && vehiclesData[company].Any(m => (string)m.model == model);
        }

        // הצגת כל החברות הקיימות במערכת
        public List<string> GetAllCarCompanies()
        {
            return ReadVehiclesListFromFile().Keys.ToList();
        }

       public List<string> GetCarModelsByCompany(string company)
{
    var vehiclesData = ReadVehiclesListFromFile();
    
    if (vehiclesData.ContainsKey(company))
    {
        return vehiclesData[company].Select(m => (string)m["model"]).ToList();
    }

    return new List<string>(); // אם החברה לא קיימת, נחזיר רשימה ריקה
}


        // קריאה מקובץ cars.json
        public List<Car> ReadCarsFromFile()
        {
            // הדפסת הנתיב הנוכחי של האפליקציה
            string currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine("Current Directory: " + currentDirectory);

            // יצירת הנתיב המלא לקובץ vehicles.json
            string fullPath = Path.Combine(currentDirectory, _carsFilePath);
            Console.WriteLine("Full file path: " + fullPath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("File not found: " + fullPath);
                return new List<Car>(); // אם הקובץ לא קיים, נחזיר רשימה ריקה
            }

            var json = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<List<Car>>(json) ?? new List<Car>(); // החזרת המידע מקובץ JSON
        }

        // שמירה ב-carsCalls.json
        public void SaveCarsCallsToFile(List<Car> cars)
        {
            var json = JsonConvert.SerializeObject(cars, Formatting.Indented);
            File.WriteAllText(_carsCallsFilePath, json);  // שמירה בקובץ carsCalls.json
        }

          // הגשה של בעיה לרכב - אם הרכב קיים ב-vehicles.json
    public IActionResult SubmitProblem(int userId, string company, string model, int year, string problemDescription)
    {
        var cars = ReadCarsFromFile();

        // בדיקת האם הרכב קיים ב-vehicles.json
        var existingCar = cars.FirstOrDefault(c => c.Company == company && c.Model == model && c.Year == year);

        if (existingCar == null)
        {
            return new BadRequestObjectResult("Car not found in the system."); // אם הרכב לא נמצא
        }

        // הוספת הבעיה לרכב
        var carCall = new Car
        {
            UserId = userId,  // קישור למשתמש
            Company = company,
            Model = model,
            Year = year,
            ProblemDescription = problemDescription
        };

        // קריאה חדשה
        var carCalls = new List<Car>();
        if (File.Exists(_carsCallsFilePath))
        {
            carCalls = JsonConvert.DeserializeObject<List<Car>>(File.ReadAllText(_carsCallsFilePath)) ?? new List<Car>();
        }

        // הוספת קריאת הרכב
        carCalls.Add(carCall);
        SaveCarsCallsToFile(carCalls);  // שמירה ב-carsCalls.json

        return new OkObjectResult("Problem submitted successfully!"); // שליחה בהצלחה
    }
    }
}

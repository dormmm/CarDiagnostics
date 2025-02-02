using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using CarDiagnostics.Models;

namespace CarDiagnostics.Services
{
    public class CarService
    {
        private readonly string _carsFilePath = "cars.json"; // קובץ הרכבים
        private readonly string _carsCallsFilePath = "cars.json"; // קובץ הקריאות

        // קריאה מקובץ cars.json
        public List<Car> ReadCarsFromFile()
        {
            if (!File.Exists(_carsFilePath))
            {
                return new List<Car>(); // אם הקובץ לא קיים, נחזיר רשימה ריקה
            }

            var json = File.ReadAllText(_carsFilePath);
            return JsonConvert.DeserializeObject<List<Car>>(json) ?? new List<Car>(); // החזרת המידע מקובץ JSON
        }

        // שמירה ב-carsCalls.json
        public void SaveCarsCallsToFile(List<Car> cars)
        {
            var json = JsonConvert.SerializeObject(cars, Formatting.Indented);
            File.WriteAllText(_carsCallsFilePath, json);  // שמירה בקובץ carsCalls.json
        }

        // הגשה של בעיה לרכב - אם הרכב קיים ב-cars.json
        public void SubmitProblem(int userId, string company, string model, int year, string problemDescription)
        {
            var cars = ReadCarsFromFile();

            // בדיקת האם הרכב קיים ב-cars.json
            var existingCar = cars.FirstOrDefault(c => c.Company == company && c.Model == model && c.Year == year);

            if (existingCar == null)
            {
                throw new System.Exception("Car not found in the system."); // אם הרכב לא נמצא
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
        }
    }
}

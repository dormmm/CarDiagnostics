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
        // נתיבי קבצים
        private readonly string _carsCallsFilePath = "carsCalls.json"; // קובץ קריאות תיקון
        private readonly string _vehiclesListFilePath = "vehiclesList.json"; // קובץ יצרנים ודגמים

        // ==============================
        //         קריאה מקבצים
        // ==============================

        /// קריאה מקובץ vehiclesList.json (יצרנים ודגמים)
        public Dictionary<string, List<dynamic>> ReadVehiclesListFromFile()
        {
            if (!File.Exists(_vehiclesListFilePath))
            {
                return new Dictionary<string, List<dynamic>>(); // אם אין קובץ, מחזירים רשימה ריקה
            }

            var json = File.ReadAllText(_vehiclesListFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, List<dynamic>>>(json) ?? new Dictionary<string, List<dynamic>>();
        }

        /// קריאה מקובץ carsCalls.json (קריאות תיקון)
        public List<Car> ReadCarsCallsFromFile()
        {
            if (!File.Exists(_carsCallsFilePath))
            {
                return new List<Car>(); 
            }

            var json = File.ReadAllText(_carsCallsFilePath);
            return JsonConvert.DeserializeObject<List<Car>>(json) ?? new List<Car>();
        }

        /// שמירת קריאות תיקון לקובץ carsCalls.json
        public void SaveCarsCallsToFile(List<Car> cars)
        {
            var json = JsonConvert.SerializeObject(cars, Formatting.Indented);
            File.WriteAllText(_carsCallsFilePath, json);  
        }

        // ==============================
        //        חיפושי נתונים
        // ==============================

        /// בדיקה אם יצרן רכב קיים במערכת
        public bool IsCompanyExists(string company)
        {
            var vehiclesData = ReadVehiclesListFromFile();
            return vehiclesData.ContainsKey(company);
        }

        /// בדיקה אם דגם מסוים קיים תחת יצרן מסוים
        public bool IsCarModelExists(string company, string model)
        {
            var vehiclesData = ReadVehiclesListFromFile();
            return vehiclesData.ContainsKey(company) && vehiclesData[company].Any(m => (string)m["model"] == model);
        }

        /// קבלת רשימת כל היצרנים הקיימים במערכת
        public List<string> GetAllCarCompanies()
        {
            return ReadVehiclesListFromFile().Keys.ToList();
        }

        /// קבלת רשימת דגמים עבור יצרן מסוים
        public List<string> GetCarModelsByCompany(string company)
        {
            var vehiclesData = ReadVehiclesListFromFile();

            if (vehiclesData.ContainsKey(company))
            {
                return vehiclesData[company].Select(m => (string)m["model"]).ToList();
            }

            return new List<string>(); // אם החברה לא קיימת, נחזיר רשימה ריקה
        }

        // ==============================
        //        שירותים עיקריים
        // ==============================

        /// הגשת קריאת תיקון עבור רכב קיים
        public IActionResult SubmitProblem(string company, string model, int year, string problemDescription)
        {
            // קריאת רשימת החברות והדגמים
            var vehiclesData = ReadVehiclesListFromFile();

            // בדיקה אם החברה והדגם קיימים
            if (!IsCompanyExists(company) || !IsCarModelExists(company, model))
            {
                return new BadRequestObjectResult("Company or model not found in the system.");
            }

            // יצירת קריאה חדשה
            var carCall = new Car
            {
                Company = company,
                Model = model,
                Year = year,
                ProblemDescription = problemDescription
            };

            // טעינת הקריאות הקיימות בקובץ
            var carCalls = ReadCarsCallsFromFile();

            // הוספת הקריאה החדשה
            carCalls.Add(carCall);

            // שמירת הקריאות חזרה לקובץ
            SaveCarsCallsToFile(carCalls);

            return new OkObjectResult("Problem submitted successfully!");
        }
    }
}

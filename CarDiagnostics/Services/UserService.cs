using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CarDiagnostics.Models;  // ייבוא המודל

namespace CarDiagnostics.Services
{
    public class UserService : IUserService
    {
        private readonly string _filePath = "users.json";  // הגדרת מיקום הקובץ

        // פונקציה לקרוא את המשתמשים מקובץ JSON
        private List<User> ReadUsersFromFile()
        {
            if (!File.Exists(_filePath))
            {
                return new List<User>(); // אם הקובץ לא קיים, נחזיר רשימה ריקה
            }

            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>(); // הפיכת המידע מ-JSON לרשימה של משתמשים
        }

        // פונקציה לשמור את המשתמשים לקובץ JSON
        private void SaveUsersToFile(List<User> users)
        {
            var json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(_filePath, json);  // שמירת המידע בקובץ
        }

        public void Register(string username, string password, string email)
        {
            var users = ReadUsersFromFile();
            var user = new User
            {
                Id = users.Count + 1,
                Username = username,
                Password = password,
                Email = email
            };

            users.Add(user);
            SaveUsersToFile(users);  // שמירה לאחר הרישום
        }

        public List<User> GetAllUsers()
        {
            return ReadUsersFromFile();  // מחזיר את כל המשתמשים מהקובץ
        }

        public void UpdateUserProfile(int userId, string name, string email, string password = null)
        {
            var users = ReadUsersFromFile();
            var user = users.Find(u => u.Id == userId);
            if (user != null)
            {
                user.Username = name;
                user.Email = email;
                if (!string.IsNullOrEmpty(password))
                {
                    user.Password = password;
                }

                SaveUsersToFile(users);  // שמירה אחרי העדכון
            }
        }
    }
}

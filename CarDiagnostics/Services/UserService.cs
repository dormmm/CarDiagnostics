using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using CarDiagnostics.Models;
using BCrypt.Net;

namespace CarDiagnostics.Services
{
    public class UserService : IUserService
    {
        private readonly string _filePath = "users.json";

        private List<User> ReadUsersFromFile()
        {
            if (!File.Exists(_filePath))
            {
                return new List<User>();
            }

            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsersToFile(List<User> users)
        {
            var json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public void Register(string username, string password, string email)
        {
            var users = ReadUsersFromFile();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password); // הצפנת הסיסמה

            var user = new User
            {
                Id = users.Count + 1,
                Username = username,
                Password = hashedPassword,
                Email = email
            };

            users.Add(user);
            SaveUsersToFile(users);
        }

        public List<User> GetAllUsers()
        {
            return ReadUsersFromFile();
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
                    user.Password = BCrypt.Net.BCrypt.HashPassword(password); // הצפנה גם בשינוי סיסמה
                }

                SaveUsersToFile(users);
            }
        }
    }
}

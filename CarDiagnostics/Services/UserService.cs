using System.Collections.Generic;
using CarDiagnostics.Models;

namespace CarDiagnostics.Services
{
    public class UserService : IUserService
    {
        private static List<User> _users = new List<User>();

        public void Register(string username, string password, string email)
        {
            var user = new User
            {
                Id = _users.Count + 1,
                Username = username,
                Password = password,
                Email = email
            };
            _users.Add(user);
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }

        public void UpdateUserProfile(int userId, string name, string email, string password = null)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Username = name;
                user.Email = email;
                if (!string.IsNullOrEmpty(password))
                {
                    user.Password = password; // אם הוספנו סיסמה חדשה, נעשה עדכון
                }
            }
        }
    }
}

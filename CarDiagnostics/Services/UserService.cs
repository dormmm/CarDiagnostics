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
    }
}

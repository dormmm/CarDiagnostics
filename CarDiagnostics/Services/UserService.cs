using System;
using System.Collections.Generic;
using System.Linq;
using CarDiagnostics.Models;

namespace CarDiagnostics.Services
{
    public class UserService : IUserService
    {
        private readonly List<User> _users = new List<User>();

        public void Register(string username, string password, string email)
        {
            if (_users.Any(u => u.Username == username))
            {
                throw new InvalidOperationException("Username already exists.");
            }

            _users.Add(new User
            {
                Username = username,
                Password = password,
                Email = email
            });
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }
    }
}

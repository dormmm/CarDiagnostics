using CarDiagnostics.Models;
using CarDiagnostics.Repository;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CarDiagnostics.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(UserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public List<User> GetAllUsers()
        {
            return _userRepository.GetAllUsers();
        }

        public void Register(string username, string password, string email)
        {
            var users = _userRepository.GetAllUsers();

            var newUser = new User
            {
                Id = users.Count + 1,
                Username = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Email = email
            };

            users.Add(newUser);

            _userRepository.SaveUsers(users);
            _logger.LogInformation("User registered: {Username}", username);
        }

        public void UpdateUserProfile(int userId, string username, string email, string password)
        {
            var users = _userRepository.GetAllUsers();
            var user = users.Find(u => u.Id == userId);
            if (user != null)
            {
                user.Username = username;
                user.Email = email;
                if (!string.IsNullOrEmpty(password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                }

                _userRepository.SaveUsers(users);
                _logger.LogInformation("User profile updated: ID {UserId}", userId);
            }
        }
    }
}

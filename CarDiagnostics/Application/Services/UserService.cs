using CarDiagnostics.Models;
using CarDiagnostics.Repository;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

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

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task RegisterAsync(string username, string password, string email)
        {
            var users = await _userRepository.GetAllUsersAsync();

            var newUser = new User
            {
                Id = users.Count + 1,
                Username = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Email = email
            };

            users.Add(newUser);
            await _userRepository.SaveUsersAsync(users);

            _logger.LogInformation("User registered: {Username}", username);
        }

        public async Task UpdateUserProfileAsync(int userId, string username, string email, string password)
        {
            var users = await _userRepository.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("Attempted to update non-existing user with ID {UserId}", userId);
                throw new KeyNotFoundException($"User with ID {userId} was not found.");
            }

            user.Username = username;
            user.Email = email;

            if (!string.IsNullOrEmpty(password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            }

            await _userRepository.SaveUsersAsync(users);
            _logger.LogInformation("User profile updated: ID {UserId}", userId);
        }

        public async Task<bool> IsValidUserAsync(string username, string email)
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Any(u => u.Username == username && u.Email == email);
        }
    }
}

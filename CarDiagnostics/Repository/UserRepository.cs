using CarDiagnostics.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace CarDiagnostics.Repository
{
    public class UserRepository
    {
        private readonly string _usersFilePath = "users.json";
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ILogger<UserRepository> logger)
        {
            _logger = logger;
        }

        public List<User> GetAllUsers()
        {
            _lock.EnterReadLock();
            try
            {
                if (!File.Exists(_usersFilePath))
                    return new List<User>();

                var json = File.ReadAllText(_usersFilePath);
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading users from file: {FilePath}", _usersFilePath);
                return new List<User>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool IsValidUser(string username, string email)
        {
            var users = GetAllUsers();
            return users.Any(u => u.Username == username && u.Email == email);
        }
    }
}

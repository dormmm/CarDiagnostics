using CarDiagnostics.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CarDiagnostics.Repository
{
    public class UserRepository
    {
        private readonly string _filePath;
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
        {
            _filePath = configuration["FilePaths:Users"] ?? throw new Exception("Missing config for FilePaths:Users");
            _logger = logger;
        }

        public List<User> GetAllUsers()
        {
            _lock.EnterReadLock();
            try
            {
                if (!File.Exists(_filePath))
                    return new List<User>();

                var json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading users from file.");
                return new List<User>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void SaveUsers(List<User> users)
        {
            _lock.EnterWriteLock();
            try
            {
                var json = JsonConvert.SerializeObject(users, Formatting.Indented);
                File.WriteAllText(_filePath, json);
                _logger.LogInformation("Users saved successfully to {FilePath}", _filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving users to file.");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool IsValidUser(string username, string email)
        {
            var users = GetAllUsers();
            return users.Any(u => u.Username == username && u.Email == email);
        }
    }
}

using CarDiagnostics.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CarDiagnostics.Domain.Interfaces;


namespace CarDiagnostics.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore = new(1, 1);  // ✅ תחליף ל-ReaderWriterLockSlim
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
        {
            _filePath = configuration["FilePaths:Users"] ?? throw new Exception("Missing config for FilePaths:Users");
            _logger = logger;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                    return new List<User>();

                var json = await File.ReadAllTextAsync(_filePath);
                return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading users from file.");
                return new List<User>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveUsersAsync(List<User> users)
        {
            await _semaphore.WaitAsync();
            try
            {
                var json = JsonConvert.SerializeObject(users, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
                _logger.LogInformation("Users saved successfully to {FilePath}", _filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving users to file.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> IsValidUserAsync(string username, string email)
        {
            var users = await GetAllUsersAsync();
            return users.Any(u => u.Username == username && u.Email == email);
        }
    }
}

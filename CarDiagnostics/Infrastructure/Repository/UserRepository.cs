using CarDiagnostics.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CarDiagnostics.Domain.Interfaces;
using CarDiagnostics.Domain.Models.Interfaces; // IAzureStorageService
using System;

namespace CarDiagnostics.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IAzureStorageService _storageService;
        private readonly string _fileName = "users.json";
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger<UserRepository> _logger;

        private static List<User>? _cachedUsers;
        private static DateTime _lastCacheTime;
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public UserRepository(IAzureStorageService storageService, ILogger<UserRepository> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            if (_cachedUsers != null && DateTime.UtcNow - _lastCacheTime < _cacheDuration)
                return _cachedUsers;

            await _semaphore.WaitAsync();
            try
            {
                var json = await _storageService.DownloadFileAsync(_fileName);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<User>();

                _cachedUsers = JsonConvert.DeserializeObject<List<User>>(json!) ?? new List<User>();
                _lastCacheTime = DateTime.UtcNow;
                return _cachedUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading users from Azure blob.");
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
                await _storageService.UploadFileAsync(_fileName, json);

                _cachedUsers = users;
                _lastCacheTime = DateTime.UtcNow;
                _logger.LogInformation("Users saved successfully to Azure blob.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving users to Azure blob.");
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
